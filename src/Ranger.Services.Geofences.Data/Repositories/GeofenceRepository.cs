using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonDiffPatchDotNet;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;
using Ranger.Common;

namespace Ranger.Services.Geofences.Data
{
    public class GeofenceRepository : IGeofenceRepository
    {
        private class Union
        {
            public IEnumerable<Geofence> CircularMatches { get; set; }
            public IEnumerable<Geofence> PolygonMatches { get; set; }
        }

        private readonly ILogger<GeofenceRepository> logger;
        private readonly IMongoCollection<Geofence> geofenceCollection;
        private readonly IMongoCollection<GeofenceChangeLog> geofenceChangeLogCollection;
        private readonly JsonDiffPatch jsonDiffPatch;

        public GeofenceRepository(IMongoDatabase database, JsonDiffPatch jsonDiffPatch, ILogger<GeofenceRepository> logger)
        {
            this.jsonDiffPatch = jsonDiffPatch;
            this.geofenceCollection = database.GetCollection<Geofence>(CollectionNames.GeofenceCollection);
            this.geofenceChangeLogCollection = database.GetCollection<GeofenceChangeLog>(CollectionNames.GeofenceChangeLogCollection);
            this.logger = logger;
        }

        public async Task AddGeofence(Geofence geofence, string commandingUserEmailOrTokenPrefix)
        {
            if (geofence is null)
            {
                throw new ArgumentNullException($"{nameof(geofence)} was null.");
            }
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace.");
            }
            await geofenceCollection.InsertOneAsync(geofence);
            await insertCreatedChangeLog(geofence, commandingUserEmailOrTokenPrefix);
        }

        public async Task UpdateGeofence(Geofence geofence, string commandingUserEmailOrTokenPrefix)
        {
            if (geofence is null)
            {
                throw new ArgumentNullException($"{nameof(geofence)} was null.");
            }
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace.");
            }

            var currentGeofence = await this.GetGeofenceAsync(geofence.PgsqlDatabaseUsername, geofence.ProjectId, geofence.Id);
            if (currentGeofence is null)
            {
                throw new RangerException($"A geofence could not be found for the provided Id '{geofence.Id}'.");
            }

            await geofenceCollection.ReplaceOneAsync(
                (_) => _.PgsqlDatabaseUsername == geofence.PgsqlDatabaseUsername && _.ProjectId == geofence.ProjectId && _.Id == geofence.Id,
                geofence
            );

            await insertUpsertedChangeLog(currentGeofence, geofence, commandingUserEmailOrTokenPrefix);
            return;
        }

        public async Task DeleteGeofence(string pgsqlDatabaseUsername, Guid projectId, string externalId, string commandingUserEmailOrTokenPrefix)
        {
            if (string.IsNullOrWhiteSpace(pgsqlDatabaseUsername))
            {
                throw new ArgumentException($"{nameof(pgsqlDatabaseUsername)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new ArgumentException($"{nameof(externalId)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace.");
            }

            var geofence = await GetGeofenceAsync(pgsqlDatabaseUsername, projectId, externalId);

            if (geofence is null)
            {
                throw new RangerException($"A geofence could not be found for the provided externalId '{externalId}'.");
            }

            await geofenceCollection.DeleteOneAsync(
                (_) => _.PgsqlDatabaseUsername == pgsqlDatabaseUsername && _.ProjectId == projectId && _.ExternalId == externalId
            );
            await insertDeletedChangeLog(pgsqlDatabaseUsername, projectId, geofence.Id, commandingUserEmailOrTokenPrefix);
        }

        public async Task<Geofence> GetGeofenceAsync(string pgsqlDatabaseUsername, Guid projectId, string externalId)
        {
            return await geofenceCollection.Aggregate()
                .Match(g => g.PgsqlDatabaseUsername == pgsqlDatabaseUsername && g.ProjectId == projectId && g.ExternalId == externalId)
                .As<Geofence>()
                .FirstOrDefaultAsync();

        }

        public async Task<Geofence> GetGeofenceAsync(string pgsqlDatabaseUsername, Guid projectId, Guid geofenceId)
        {
            return await geofenceCollection.Aggregate()
                .Match(g => g.PgsqlDatabaseUsername == pgsqlDatabaseUsername && g.ProjectId == projectId && g.Id == geofenceId)
                .As<Geofence>()
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<GeofenceResponseModel>> GetGeofencesContainingLocation(string pgsqlDatabaseUsername, Guid projectId, LngLat lngLat, double accuracy)
        {
            var circularLookup = new BsonDocument{
                {"$lookup", new BsonDocument{
                    {"from", "geofences"},
                    {"let", new BsonDocument{}},
                    {"pipeline", GetCircularSubPipeline(pgsqlDatabaseUsername, lngLat)},
                    {"as", "CircularMatches"}
                }}
            };
            var polygonLookup = new BsonDocument{
                {"$lookup", new BsonDocument{
                    {"from", "geofences"},
                    {"let", new BsonDocument{}},
                    {"pipeline", GetPolygonSubPipeline(pgsqlDatabaseUsername, lngLat)},
                    {"as", "PolygonMatches"}
                }}
            };

            var intersectingGeofences = await geofenceCollection.Aggregate()
                .Limit(1)
                .Project("{_id:1}")
                .Project("{_id:0}")
                .AppendStage<Union>(circularLookup)
                .AppendStage<Union>(polygonLookup)
                .Project(new BsonDocument{
                    {"Union", new BsonDocument{
                        {"$concatArrays", new BsonArray{"$CircularMatches", "$PolygonMatches"}}
                    }}
                })
                .Unwind(x => x["Union"])
                .ReplaceRoot<Geofence>("$Union")
                .ToListAsync();

            return default;
        }

        private static BsonArray GetCircularSubPipeline(string pgsqlDatabaseUsername, LngLat lngLat)
        {
            // https://stackoverflow.com/questions/46090741/how-to-write-union-queries-in-mongodb
            var circularSubPipeline = new BsonArray();
            circularSubPipeline.Add(
                new BsonDocument{
                    {"$geoNear", new BsonDocument{
                        {"near", new BsonDocument{
                            {"type", "Point"},
                            {"coordinates", new BsonArray { lngLat.Lng, lngLat.Lat }}
                        }},
                        {"distanceField", "dist.calculated"},
                        {"maxDistance", 10000},
                        {"query", new BsonDocument{
                            {"Shape", 0}
                        }},
                        {"spherical", true}
                    }}});
            circularSubPipeline.Add(
                new BsonDocument{
                    {"$project", new BsonDocument{
                        {"_id", 1},
                        {"PgsqlDatabaseUsername", 1},
                        {"CreatedDate", 1},
                        {"UpdatedDate", 1},
                        {"Shape", 1},
                        {"GeoJsonGeometry", 1},
                        {"PolygonCentroid", 1},
                        {"Radius", 1},
                        {"ExternalId", 1},
                        {"ProjectId", 1},
                        {"Description", 1},
                        {"Labels", 1},
                        {"IntegrationIds", 1},
                        {"Metadata", 1},
                        {"OnEnter", 1},
                        {"OnExit", 1},
                        {"Enabled", 1},
                        {"ExpirationDate", 1},
                        {"LaunchDate", 1},
                        {"Schedule", 1},
                        {"TimeZoneId", 1},
                        {"CalculatedDiff", new BsonDocument{
                            {"$subtract", new BsonArray{"$dist.calculated","$Radius"}}
                        }},
                    }}});

            circularSubPipeline.Add(
                new BsonDocument{
                    {"$match", new BsonDocument{
                        {"CalculatedDiff", new BsonDocument{
                            {"$lte", 0}
                        }},
                        {"PgsqlDatabaseUsername", pgsqlDatabaseUsername}
                    }}});

            circularSubPipeline.Add(
                new BsonDocument{
                    {"$project", new BsonDocument{
                        {"_id", 1},
                        {"PgsqlDatabaseUsername", 1},
                        {"CreatedDate", 1},
                        {"UpdatedDate", 1},
                        {"Shape", 1},
                        {"GeoJsonGeometry", 1},
                        {"PolygonCentroid", 1},
                        {"Radius", 1},
                        {"ExternalId", 1},
                        {"ProjectId", 1},
                        {"Description", 1},
                        {"Labels", 1},
                        {"IntegrationIds", 1},
                        {"Metadata", 1},
                        {"OnEnter", 1},
                        {"OnExit", 1},
                        {"Enabled", 1},
                        {"ExpirationDate", 1},
                        {"LaunchDate", 1},
                        {"Schedule", 1},
                        {"TimeZoneId", 1}
                    }}});
            return circularSubPipeline;
        }

        private static BsonArray GetPolygonSubPipeline(string pgsqlDatabaseUsername, LngLat lngLat)
        {
            // https://stackoverflow.com/questions/46090741/how-to-write-union-queries-in-mongodb
            var polygonSubPipeline = new BsonArray();
            polygonSubPipeline.Add(
                new BsonDocument{
                    {"$match", new BsonDocument{
                        {"GeoJsonGeometry", new BsonDocument{
                            {"$geoIntersects", new BsonDocument{
                                {"$geometry", new BsonDocument{
                                    {"type", "Point"},
                                    {"coordinates", new BsonArray { lngLat.Lng, lngLat.Lat }}
                                }}
                            }}
                        }},
                        {"PgsqlDatabaseUsername", pgsqlDatabaseUsername}
                    }}
                });
            return polygonSubPipeline;
        }

        public async Task<IEnumerable<GeofenceResponseModel>> GetAllGeofencesByProjectId(string pgsqlDatabaseUsername, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(pgsqlDatabaseUsername))
            {
                throw new ArgumentException($"{nameof(pgsqlDatabaseUsername)} was null or whitespace.");
            }

            var geofences = await geofenceCollection.Aggregate()
                .Match(g => g.PgsqlDatabaseUsername == pgsqlDatabaseUsername && g.ProjectId == projectId)
                .Project("{_id:1,Description:1,Enabled:1,ExpirationDate:1,ExternalId:1,GeoJsonGeometry:1,IntegrationIds:1,Labels:1,LaunchDate:1,Metadata:1,OnEnter:1,OnExit:1,ProjectId:1,Radius:1,Schedule:1,Shape:1}")
                .Sort("{ExternalId:1}")
                .As<Geofence>()
                .ToListAsync();

            var geofenceResponse = new List<GeofenceResponseModel>();
            foreach (var geofence in geofences)
            {
                geofenceResponse.Add(new GeofenceResponseModel
                {
                    Id = geofence.Id,
                    Enabled = geofence.Enabled,
                    Description = geofence.Description,
                    ExpirationDate = geofence.ExpirationDate,
                    ExternalId = geofence.ExternalId,
                    Coordinates = getCoordinatesByShape(geofence.Shape, geofence.GeoJsonGeometry),
                    IntegrationIds = geofence.IntegrationIds,
                    Labels = geofence.Labels,
                    LaunchDate = geofence.LaunchDate,
                    Metadata = geofence.Metadata,
                    OnEnter = geofence.OnEnter,
                    OnExit = geofence.OnExit,
                    ProjectId = geofence.ProjectId,
                    Radius = geofence.Radius,
                    Schedule = geofence.Schedule,
                    Shape = geofence.Shape
                });
            }

            return geofenceResponse;
        }

        private IEnumerable<LngLat> getCoordinatesByShape(GeofenceShapeEnum shape, GeoJsonGeometry<GeoJson2DGeographicCoordinates> geoJsonGeometry)
        {
            switch (shape)
            {
                case GeofenceShapeEnum.Circle:
                    {
                        var point = (geoJsonGeometry as GeoJsonPoint<GeoJson2DGeographicCoordinates>);
                        return new LngLat[] { new LngLat(point.Coordinates.Longitude, point.Coordinates.Latitude) };
                    }
                case GeofenceShapeEnum.Polygon:
                    {
                        var count = (geoJsonGeometry as GeoJsonPolygon<GeoJson2DGeographicCoordinates>).Coordinates.Exterior.Positions.Count();
                        var points = (geoJsonGeometry as GeoJsonPolygon<GeoJson2DGeographicCoordinates>).Coordinates.Exterior.Positions.Take(count - 1).Select(_ => new LngLat(_.Longitude, _.Latitude));
                        return points;
                    }
                default:
                    return new LngLat[0];
            }
        }

        private async Task insertDeletedChangeLog(string pgsqlDatabaseUsername, Guid projectId, Guid geofenceId, string commandingUserEmailOrTokenPrefix)
        {
            var changeLog = new GeofenceChangeLog(Guid.NewGuid(), pgsqlDatabaseUsername);
            changeLog.GeofenceId = geofenceId;
            changeLog.ProjectId = projectId;
            changeLog.CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;
            changeLog.Event = "GeofenceDeleted";
            await geofenceChangeLogCollection.InsertOneAsync(changeLog);
        }

        private async Task insertCreatedChangeLog(Geofence geofence, string commandingUserEmailOrTokenPrefix)
        {
            var changeLog = new GeofenceChangeLog(Guid.NewGuid(), geofence.PgsqlDatabaseUsername);
            changeLog.GeofenceId = geofence.Id;
            changeLog.ProjectId = geofence.ProjectId;
            changeLog.CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;
            changeLog.Event = "GeofenceCreated";
            changeLog.GeofenceDiff = JsonConvert.SerializeObject(geofence);
            await geofenceChangeLogCollection.InsertOneAsync(changeLog);
        }

        private async Task insertUpsertedChangeLog(Geofence currentGeofence, Geofence updatedGeofence, string commandingUserEmailOrTokenPrefix)
        {
            var changeLog = new GeofenceChangeLog(Guid.NewGuid(), updatedGeofence.PgsqlDatabaseUsername);
            if (currentGeofence != null)
            {
                var diff = this.jsonDiffPatch.Diff(JsonConvert.SerializeObject(currentGeofence), JsonConvert.SerializeObject(updatedGeofence));
                changeLog.GeofenceDiff = diff;
            }
            else
            {
                changeLog.GeofenceDiff = JsonConvert.SerializeObject(updatedGeofence);

            }
            changeLog.GeofenceId = updatedGeofence.Id;
            changeLog.ProjectId = updatedGeofence.ProjectId;
            changeLog.CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;
            changeLog.Event = "GeofenceUpserted";
            await geofenceChangeLogCollection.InsertOneAsync(changeLog);
        }
    }
}
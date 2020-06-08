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
                throw new ArgumentNullException($"{nameof(geofence)} was null");
            }
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace");
            }
            await geofenceCollection.InsertOneAsync(geofence);
            await insertCreatedChangeLog(geofence, commandingUserEmailOrTokenPrefix);
        }

        public async Task UpdateGeofence(Geofence geofence, string commandingUserEmailOrTokenPrefix)
        {
            if (geofence is null)
            {
                throw new ArgumentNullException($"{nameof(geofence)} was null");
            }
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace");
            }

            var currentGeofence = await this.GetGeofenceAsync(geofence.TenantId, geofence.ProjectId, geofence.Id);
            if (currentGeofence is null)
            {
                throw new RangerException($"A geofence could not be found for the provided Id '{geofence.Id}'");
            }

            await geofenceCollection.ReplaceOneAsync(
                (_) => _.TenantId == geofence.TenantId && _.ProjectId == geofence.ProjectId && _.Id == geofence.Id,
                geofence
            );

            await insertUpsertedChangeLog(currentGeofence, geofence, commandingUserEmailOrTokenPrefix);
            return;
        }

        public async Task DeleteGeofence(string tenantId, Guid projectId, string externalId, string commandingUserEmailOrTokenPrefix)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new ArgumentException($"{nameof(externalId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace");
            }

            var geofence = await GetGeofenceAsync(tenantId, projectId, externalId);

            if (geofence is null)
            {
                throw new RangerException($"A geofence could not be found for the provided externalId '{externalId}'");
            }

            await geofenceCollection.DeleteOneAsync(
                (_) => _.TenantId == tenantId && _.ProjectId == projectId && _.ExternalId == externalId
            );
            await insertDeletedChangeLog(tenantId, projectId, geofence.Id, commandingUserEmailOrTokenPrefix);
        }

        public async Task<Geofence> GetGeofenceAsync(string tenantId, Guid projectId, string externalId)
        {
            return await geofenceCollection.Aggregate()
                .Match(g => g.TenantId == tenantId && g.ProjectId == projectId && g.ExternalId == externalId)
                .As<Geofence>()
                .FirstOrDefaultAsync();

        }

        public async Task<Geofence> GetGeofenceAsync(string tenantId, Guid projectId, Guid geofenceId)
        {
            return await geofenceCollection.Aggregate()
                .Match(g => g.TenantId == tenantId && g.ProjectId == projectId && g.Id == geofenceId)
                .As<Geofence>()
                .FirstOrDefaultAsync();
        }

        public async Task PurgeIntegrationFromAllGeofences(string tenantId, Guid projectId, Guid integrationId)
        {
            var pullFilter = Builders<Geofence>.Update.PullAll(f => f.IntegrationIds, new Guid[] { integrationId });
            var result = await geofenceCollection.UpdateManyAsync(g => g.TenantId == tenantId && g.ProjectId == projectId, pullFilter);
        }

        public async Task<IEnumerable<Geofence>> GetGeofencesAsync(string tenantId, Guid projectId, IEnumerable<Guid> geofenceIds)
        {
            return await geofenceCollection.Aggregate()
                .Match(g => g.TenantId == tenantId && g.ProjectId == projectId && geofenceIds.Contains(g.Id))
                .ToListAsync();
        }

        public async Task<long> GetAllActiveGeofencesCountAsync(string tenantId, IEnumerable<Guid> projectIds)
        {
            var result = await geofenceCollection.Aggregate()
                .Match(g => g.TenantId == tenantId && projectIds.Contains(g.ProjectId))
                .Count()
                .SingleOrDefaultAsync();
            return result?.Count ?? 0;
        }

        public async Task<IEnumerable<Geofence>> GetAllActiveGeofencesForProjectIdsAsync(string tenantId, IEnumerable<Guid> projectIds)
        {
            return await geofenceCollection.Aggregate()
                .Match(g => g.TenantId == tenantId && projectIds.Contains(g.ProjectId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Geofence>> GetGeofencesContainingLocation(string tenantId, Guid projectId, LngLat lngLat, double accuracy)
        {
            var circularLookup = new BsonDocument{
                {"$lookup", new BsonDocument{
                    {"from", "geofences"},
                    {"let", new BsonDocument{}},
                    {"pipeline", GetCircularSubPipeline(tenantId, projectId, lngLat)},
                    {"as", "CircularMatches"}
                }}
            };
            var polygonLookup = new BsonDocument{
                {"$lookup", new BsonDocument{
                    {"from", "geofences"},
                    {"let", new BsonDocument{}},
                    {"pipeline", GetPolygonSubPipeline(tenantId, projectId, lngLat)},
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

            return intersectingGeofences;
        }

        private static BsonArray GetCircularSubPipeline(string tenantId, Guid projectId, LngLat lngLat, double accuracy = 0)
        {
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
                        {"TenantId", 1},
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
                        {"OnDwell", 1},
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
                        {"TenantId", tenantId},
                        {"ProjectId", projectId}
                    }}});

            circularSubPipeline.Add(
                new BsonDocument{
                    {"$project", new BsonDocument{
                        {"_id", 1},
                        {"TenantId", 1},
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
                        {"OnDwell", 1},
                        {"OnExit", 1},
                        {"Enabled", 1},
                        {"ExpirationDate", 1},
                        {"LaunchDate", 1},
                        {"Schedule", 1},
                        {"TimeZoneId", 1}
                    }}});
            return circularSubPipeline;
        }

        private static BsonArray GetPolygonSubPipeline(string tenantId, Guid projectId, LngLat lngLat)
        {
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
                        {"TenantId", tenantId},
                        {"ProjectId", projectId}
                    }}
                });
            return polygonSubPipeline;
        }

        public async Task<IEnumerable<Geofence>> GetAllGeofencesByProjectId(string tenantId, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }

            var geofences = await geofenceCollection.Aggregate()
                .Match(g => g.TenantId == tenantId && g.ProjectId == projectId)
                .Project("{_id:1,Description:1,Enabled:1,ExpirationDate:1,ExternalId:1,GeoJsonGeometry:1,IntegrationIds:1,Labels:1,LaunchDate:1,Metadata:1,OnEnter:1,OnDwell:1,OnExit:1,ProjectId:1,Radius:1,Schedule:1,Shape:1}")
                .Sort("{ExternalId:1}")
                .As<Geofence>()
                .ToListAsync();

            return geofences;
        }


        private async Task insertDeletedChangeLog(string tenantId, Guid projectId, Guid geofenceId, string commandingUserEmailOrTokenPrefix)
        {
            var changeLog = new GeofenceChangeLog(Guid.NewGuid(), tenantId);
            changeLog.GeofenceId = geofenceId;
            changeLog.ProjectId = projectId;
            changeLog.CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;
            changeLog.Event = "GeofenceDeleted";
            await geofenceChangeLogCollection.InsertOneAsync(changeLog);
        }

        private async Task insertCreatedChangeLog(Geofence geofence, string commandingUserEmailOrTokenPrefix)
        {
            var changeLog = new GeofenceChangeLog(Guid.NewGuid(), geofence.TenantId);
            changeLog.GeofenceId = geofence.Id;
            changeLog.ProjectId = geofence.ProjectId;
            changeLog.CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;
            changeLog.Event = "GeofenceCreated";
            changeLog.GeofenceDiff = JsonConvert.SerializeObject(geofence);
            await geofenceChangeLogCollection.InsertOneAsync(changeLog);
        }

        private async Task insertUpsertedChangeLog(Geofence currentGeofence, Geofence updatedGeofence, string commandingUserEmailOrTokenPrefix)
        {
            var changeLog = new GeofenceChangeLog(Guid.NewGuid(), updatedGeofence.TenantId);
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
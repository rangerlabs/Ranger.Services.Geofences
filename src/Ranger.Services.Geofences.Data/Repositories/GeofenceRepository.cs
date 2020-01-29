using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonDiffPatchDotNet;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;
using Ranger.Common;

namespace Ranger.Services.Geofences.Data
{
    public class GeofenceRepository : IGeofenceRepository
    {
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

        public async Task DeleteGeofence(string pgsqlDatabaseUsername, string projectId, string externalId, string commandingUserEmailOrTokenPrefix)
        {
            if (string.IsNullOrWhiteSpace(pgsqlDatabaseUsername))
            {
                throw new ArgumentException($"{nameof(pgsqlDatabaseUsername)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(projectId))
            {
                throw new ArgumentException($"{nameof(projectId)} was null or whitespace.");
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

        public async Task<Geofence> GetGeofenceAsync(string pgsqlDatabaseUsername, string projectId, string externalId)
        {
            return await geofenceCollection.Aggregate()
                .Match(g => g.PgsqlDatabaseUsername == pgsqlDatabaseUsername && g.ProjectId == projectId && g.ExternalId == externalId)
                .As<Geofence>()
                .FirstOrDefaultAsync();

        }

        public async Task<Geofence> GetGeofenceAsync(string pgsqlDatabaseUsername, string projectId, Guid geofenceId)
        {
            return await geofenceCollection.Aggregate()
                .Match(g => g.PgsqlDatabaseUsername == pgsqlDatabaseUsername && g.ProjectId == projectId && g.Id == geofenceId)
                .As<Geofence>()
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<GeofenceResponseModel>> GetAllGeofencesByProjectId(string pgsqlDatabaseUsername, string projectId)
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

            var geofenceResponse = geofences.Select(_ =>
               new GeofenceResponseModel()
               {
                   Id = _.Id.ToString(),
                   Enabled = _.Enabled,
                   Description = _.Description,
                   ExpirationDate = _.ExpirationDate,
                   ExternalId = _.ExternalId,
                   Coordinates = getCoordinatesByShape(_.Shape, _.GeoJsonGeometry),
                   IntegrationIds = _.IntegrationIds,
                   Labels = _.Labels,
                   LaunchDate = _.LaunchDate,
                   Metadata = _.Metadata,
                   OnEnter = _.OnEnter,
                   OnExit = _.OnExit,
                   ProjectId = _.ProjectId,
                   Radius = _.Radius,
                   Schedule = _.Schedule,
                   Shape = _.Shape
               }
            );
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

        private async Task insertDeletedChangeLog(string pgsqlDatabaseUsername, string projectId, Guid geofenceId, string commandingUserEmailOrTokenPrefix)
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
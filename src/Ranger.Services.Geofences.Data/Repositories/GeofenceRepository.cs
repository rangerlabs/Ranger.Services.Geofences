using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Ranger.Common;
using Ranger.Mongo;

namespace Ranger.Services.Geofences.Data
{
    public class GeofenceRepository : IGeofenceRepository
    {
        private readonly ILogger<GeofenceRepository> logger;
        private readonly IMongoCollection<Geofence> collection;

        public GeofenceRepository(IMongoDatabase database, ILogger<GeofenceRepository> logger)
        {
            this.collection = database.GetCollection<Geofence>(CollectionNames.GeofenceCollection);
            this.logger = logger;
        }

        public async Task AddGeofence(Geofence geofence)
        {
            await collection.InsertOneAsync(geofence);
        }

        public async Task<IEnumerable<GeofenceResponseModel>> GetAllGeofencesByProjectId(string pgsqlDatabaseUsername, string projectId)
        {
            if (string.IsNullOrWhiteSpace(pgsqlDatabaseUsername))
            {
                throw new ArgumentException($"{nameof(pgsqlDatabaseUsername)} was null or whitespace.");
            }

            var geofences = await collection.Aggregate()
            .Match(g => g.PgsqlDatabaseUsername == pgsqlDatabaseUsername && g.ProjectId == projectId)
            .Project("{_id:0,Description:1,Enabled:1,ExpirationDate:1,ExternalId:1,GeoJsonGeometry:1,IntegrationIds:1,Labels:1,LaunchDate:1,Metadata:1,OnEnter:1,OnExit:1,ProjectId:1,Radius:1,Schedule:1,Shape:1}")
            .Sort("{ExternalId:1}")
            .As<Geofence>()
            .ToListAsync();

            return geofences.Select(_ =>
                new GeofenceResponseModel()
                {
                    Enabled = _.Enabled,
                    Description = _.Description,
                    ExpirationDate = _.ExpirationDate,
                    ExternalId = _.ExternalId,
                    LngLat = new LngLat(90, 180),
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
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Ranger.Mongo;

namespace Ranger.Services.Geofences.Data {
    public class GeofenceSeeder : IMongoDbSeeder {
        private readonly IMongoCollection<Geofence> geofencesCollection;
        private readonly IMongoCollection<GeofenceChangeLog> geofenceChangeLogsCollection;
        private readonly ILogger<IMongoDbSeeder> logger;

        public GeofenceSeeder (IMongoDatabase mongoDatabase, ILogger<IMongoDbSeeder> logger) {
            geofencesCollection = mongoDatabase.GetCollection<Geofence> (CollectionNames.GeofenceCollection);
            geofenceChangeLogsCollection = mongoDatabase.GetCollection<GeofenceChangeLog> (CollectionNames.GeofenceChangeLogCollection);
            this.logger = logger;
        }

        public async Task SeedAsync () {
            logger.LogInformation ($"Adding geofence indices");
            await GeofenceCollectionIndices ();
            await GeofenceChangeLogCollectionIndices ();
        }

        private async Task GeofenceChangeLogCollectionIndices () {
            logger.LogInformation ($"Adding indices to {CollectionNames.GeofenceChangeLogCollection} collection");
            try {
                IList<CreateIndexModel<GeofenceChangeLog>> indexModels = new List<CreateIndexModel<GeofenceChangeLog>> ();
                indexModels.Add (new CreateIndexModel<GeofenceChangeLog> (Builders<GeofenceChangeLog>.IndexKeys.Ascending (_ => _.TenantId).Ascending (_ => _.ProjectId).Ascending (_ => _.GeofenceId)));
                await geofenceChangeLogsCollection.Indexes.CreateManyAsync (indexModels);
            } catch (Exception ex) {
                logger.LogError (ex, $"Failed to add indices to {CollectionNames.GeofenceChangeLogCollection} collection");
                throw;
            }
        }

        private async Task GeofenceCollectionIndices () {
            logger.LogInformation ($"Adding indices to {CollectionNames.GeofenceCollection} collection");
            try {
                IList<CreateIndexModel<Geofence>> indexModels = new List<CreateIndexModel<Geofence>> ();
                indexModels.Add (new CreateIndexModel<Geofence> (Builders<Geofence>.IndexKeys.Ascending (_ => _.Shape)));
                indexModels.Add (new CreateIndexModel<Geofence> (Builders<Geofence>.IndexKeys.Geo2DSphere (_ => _.GeoJsonGeometry)));
                indexModels.Add (new CreateIndexModel<Geofence> (Builders<Geofence>.IndexKeys.Ascending (_ => _.ProjectId).Ascending (_ => _.ExternalId), new CreateIndexOptions () { Unique = true }));
                indexModels.Add (new CreateIndexModel<Geofence> (Builders<Geofence>.IndexKeys.Ascending (_ => _.TenantId).Ascending (_ => _.ProjectId)));
                await geofencesCollection.Indexes.CreateManyAsync (indexModels);
            } catch (Exception ex) {
                logger.LogError (ex, $"Failed to add indices to {CollectionNames.GeofenceCollection} collection");
                throw;
            }
        }
    }
}
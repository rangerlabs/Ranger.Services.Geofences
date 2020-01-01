using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Ranger.Mongo;

namespace Ranger.Services.Geofences.Data
{
    public class GeofenceSeeder : IMongoDbSeeder
    {
        private readonly IMongoCollection<Geofence> collection;
        private readonly ILogger<IMongoDbSeeder> logger;

        public GeofenceSeeder(IMongoDatabase mongoDatabase, ILogger<IMongoDbSeeder> logger)
        {
            collection = mongoDatabase.GetCollection<Geofence>(CollectionNames.GeofenceCollection);
            this.logger = logger;
        }

        public async Task SeedAsync()
        {
            logger.LogInformation($"Adding indices to {CollectionNames.GeofenceCollection} collection.");
            try
            {
                IList<CreateIndexModel<Geofence>> indexModels = new List<CreateIndexModel<Geofence>>();
                indexModels.Add(new CreateIndexModel<Geofence>(Builders<Geofence>.IndexKeys.Ascending(_ => _.Shape)));
                indexModels.Add(new CreateIndexModel<Geofence>(Builders<Geofence>.IndexKeys.Geo2DSphere(_ => _.GeoJsonGeometry)));
                indexModels.Add(new CreateIndexModel<Geofence>(Builders<Geofence>.IndexKeys.Ascending(_ => _.ExternalId).Ascending(_ => _.ProjectId), new CreateIndexOptions() { Unique = true }));
                indexModels.Add(new CreateIndexModel<Geofence>(Builders<Geofence>.IndexKeys.Ascending(_ => _.ProjectId).Ascending(_ => _.PgsqlDatabaseUsername)));
                await collection.Indexes.CreateManyAsync(indexModels);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to add indices to {CollectionNames.GeofenceCollection} collection.");
                throw;
            }
            logger.LogInformation($"Indices added successfully.");
        }
    }
}
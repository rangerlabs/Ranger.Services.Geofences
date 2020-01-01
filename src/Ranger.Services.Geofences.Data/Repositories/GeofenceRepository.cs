using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
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
    }
}
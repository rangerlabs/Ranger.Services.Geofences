using System.Threading.Tasks;
using MongoDB.Driver;
using Ranger.Mongo;

namespace Ranger.Services.Geofences.Data
{

    public class GeofenceRepository : IGeofenceRepository
    {
        private IMongoCollection<Geofence> collection { get; }

        public GeofenceRepository(IMongoDatabase database)
        {
            collection = database.GetCollection<Geofence>(CollectionNames.GeofenceCollection);
        }

        public async Task AddGeofence(Geofence geofence)
            => await collection.InsertOneAsync(geofence);
    }
}
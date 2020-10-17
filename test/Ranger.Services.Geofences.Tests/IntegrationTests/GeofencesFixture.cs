using System;
using MongoDB.Driver;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences.Tests.IntegrationTests
{
    public class GeofencesFixture : IDisposable
    {
        private IMongoCollection<Geofence> geofenceCollection;
        private IMongoCollection<GeofenceChangeLog> geofenceChangeLogCollection;

        public void SeedMongoDb(IGeofenceRepository geofenceRepository)
        {
            this.geofenceCollection = geofenceRepository.Database.GetCollection<Geofence>(CollectionNames.GeofenceCollection);
            this.geofenceChangeLogCollection = geofenceRepository.Database.GetCollection<GeofenceChangeLog>(CollectionNames.GeofenceChangeLogCollection);
            Seed.SeedGeofences(geofenceRepository);
        }

        public void Dispose()
        {
            geofenceCollection.DeleteMany((_) => true);
            geofenceChangeLogCollection.DeleteMany((_) => true);
        }
    }
}
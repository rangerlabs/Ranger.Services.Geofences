using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences.Tests.IntegrationTests
{
    public static class Seed
    {
        public static void SeedGeofences(IGeofenceRepository repo)
        {
            foreach (var geofence in TenantOneGeofences.Project1Geofences()) {
                repo.AddGeofence(geofence, "test");
            }
            foreach (var geofence in TenantOneGeofences.Project2Geofences()) {
                repo.AddGeofence(geofence, "test");
            }
            foreach (var geofence in TenantTwoGeofences.Project1Geofences()) {
                repo.AddGeofence(geofence, "test");
            }
            foreach (var geofence in TenantTwoGeofences.Project2Geofences()) {
                repo.AddGeofence(geofence, "test");
            }
        }
    }
}
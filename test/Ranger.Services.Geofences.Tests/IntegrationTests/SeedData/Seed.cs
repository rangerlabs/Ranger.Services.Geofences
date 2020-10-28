using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ranger.Common;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences.Tests.IntegrationTests
{
    public static class Seed
    {

        public static string TenantId1 => "tenant-one";
        public static Guid TenantId1_ProjectId1 => Guid.Parse("e9f6da74-87d1-4d96-a7fb-df44906be274");
        public static Guid TenantId1_ProjectId2 => Guid.Parse("90b45447-4850-42b1-ac17-dc36d0902e26");

        public static string TenantId2 => "tenant-two";
        public static Guid TenantId2_ProjectId1 => Guid.Parse("26adc534-ea86-473c-8ea2-f929f5ef4e2f");
        public static Guid TenantId2_ProjectId2 => Guid.Parse("7aab5eef-af84-470c-b32b-855d431cae22");

        public static IList<Geofence> TenantId1_ProjectId1_Geofences = new List<Geofence>();
        public static IList<Geofence> TenantId1_ProjectId2_Geofences = new List<Geofence>();
        public static IList<Geofence> TenantId2_ProjectId1_Geofences = new List<Geofence>();
        public static IList<Geofence> TenantId2_ProjectId2_Geofences = new List<Geofence>();

        public static void SeedGeofences(IGeofenceRepository repo)
        {
            for(int i = 0; i <= 100; i++) {
                var geofence = GenerateGeofence(TenantId1, TenantId1_ProjectId1, $"tenants-1-project-1-geofence-{i}");
                TenantId1_ProjectId1_Geofences.Add(geofence);
                repo.AddGeofence(geofence, "test");
                Thread.Sleep(1); //Force increment of CreatedDate
            }
            for(int i = 0; i <= 100; i++) {
                var geofence = GenerateGeofence(TenantId1, TenantId1_ProjectId2, $"tenants-1-project-2-geofence-{i}");
                TenantId1_ProjectId2_Geofences.Add(geofence);
                repo.AddGeofence(geofence, "test");
                Thread.Sleep(1); //Force increment of CreatedDate
            }
            for(int i = 0; i <= 100; i++) {
                var geofence = GenerateGeofence(TenantId2, TenantId2_ProjectId1, $"tenants-2-project-1-geofence-{i}");
                TenantId2_ProjectId1_Geofences.Add(geofence);
                repo.AddGeofence(geofence, "test");
                Thread.Sleep(1); //Force increment of CreatedDate
            }
            for(int i = 0; i <= 100; i++) {
                var geofence = GenerateGeofence(TenantId2, TenantId2_ProjectId2, $"tenants-2-project-2-geofence-{i}");
                TenantId2_ProjectId2_Geofences.Add(geofence);
                repo.AddGeofence(geofence, "test");
                Thread.Sleep(1); //Force increment of CreatedDate
            }
       }

        private static Geofence GenerateGeofence(string tenantId, Guid projectId, string externalId)
        {
            return new Geofence(
                    Guid.NewGuid(),
                    tenantId,
                    GeofenceShapeEnum.Circle,
                    GeoJsonGeometryFactory.Factory(GeofenceShapeEnum.Circle, new List<LngLat> { new LngLat(-81.5576475444844, 41.48761481059155) }),
                    default,
                    100,
                    externalId,
                    projectId,
                    "",
                    true,
                    false,
                    true,
                    true);
        }
    }
}
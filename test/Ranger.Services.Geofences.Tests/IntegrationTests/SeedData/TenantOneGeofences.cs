using System;
using System.Collections.Generic;
using Ranger.Common;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences.Tests.IntegrationTests
{
    public static class TenantOneGeofences
    {
        public static string TenantId => "tenant-one";
        public static Guid ProjectId1 => Guid.Parse("e9f6da74-87d1-4d96-a7fb-df44906be274");
        public static Guid ProjectId2 => Guid.Parse("90b45447-4850-42b1-ac17-dc36d0902e26");

        public static IEnumerable<Geofence> Project1Geofences() {
            return new List<Geofence> 
            {
                new Geofence(
                    Guid.NewGuid(),
                    TenantId,
                    GeofenceShapeEnum.Circle,
                    GeoJsonGeometryFactory.Factory(GeofenceShapeEnum.Circle, new List<LngLat> { new LngLat(-81.5576475444844, 41.48761481059155) }),
                    default,
                    100,
                    "heights1",
                    ProjectId1,
                    "",
                    true,
                    false,
                    true,
                    true),

                new Geofence(
                    Guid.NewGuid(),
                    TenantId,
                    GeofenceShapeEnum.Circle,
                    GeoJsonGeometryFactory.Factory(GeofenceShapeEnum.Circle, new List<LngLat> { new LngLat(-81.5576475444844, 41.48761481059155) }),
                    default,
                    100,
                    "heights2",
                    ProjectId1,
                    "",
                    true,
                    false,
                    true,
                    true),

                new Geofence(
                    Guid.NewGuid(),
                    TenantId,
                    GeofenceShapeEnum.Circle,
                    GeoJsonGeometryFactory.Factory(GeofenceShapeEnum.Circle, new List<LngLat> { new LngLat(-81.5576475444844, 41.48761481059155) }),
                    default,
                    100,
                    "heights3",
                    ProjectId1,
                    "",
                    true,
                    false,
                    true,
                    true)
            };
        }

        public static IEnumerable<Geofence> Project2Geofences() {
            return new List<Geofence> 
            {
                new Geofence(
                    Guid.NewGuid(),
                    TenantId,
                    GeofenceShapeEnum.Circle,
                    GeoJsonGeometryFactory.Factory(GeofenceShapeEnum.Circle, new List<LngLat> { new LngLat(-81.5576475444844, 41.48761481059155) }),
                    default,
                    100,
                    "heights1",
                    ProjectId2,
                    "",
                    true,
                    false,
                    true,
                    true),

                new Geofence(
                    Guid.NewGuid(),
                    TenantId,
                    GeofenceShapeEnum.Circle,
                    GeoJsonGeometryFactory.Factory(GeofenceShapeEnum.Circle, new List<LngLat> { new LngLat(-81.5576475444844, 41.48761481059155) }),
                    default,
                    100,
                    "heights2",
                    ProjectId2,
                    "",
                    true,
                    false,
                    true,
                    true),

                new Geofence(
                    Guid.NewGuid(),
                    TenantId,
                    GeofenceShapeEnum.Circle,
                    GeoJsonGeometryFactory.Factory(GeofenceShapeEnum.Circle, new List<LngLat> { new LngLat(-81.5576475444844, 41.48761481059155) }),
                    default,
                    100,
                    "heights3",
                    ProjectId2,
                    "",
                    true,
                    false,
                    true,
                    true)
            };
        }
    }
}
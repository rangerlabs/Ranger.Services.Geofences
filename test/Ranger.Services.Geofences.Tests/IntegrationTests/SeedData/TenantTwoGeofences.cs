using System;
using System.Collections.Generic;
using Ranger.Common;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences.Tests.IntegrationTests
{
    public static class TenantTwoGeofences
    {
        public static string TenantId => "tenant-two";
        public static Guid ProjectId1 => Guid.Parse("26adc534-ea86-473c-8ea2-f929f5ef4e2f");
        public static Guid ProjectId2 => Guid.Parse("7aab5eef-af84-470c-b32b-855d431cae22");

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
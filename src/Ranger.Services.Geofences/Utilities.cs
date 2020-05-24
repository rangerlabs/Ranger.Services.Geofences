using System.Collections.Generic;
using System.Linq;
using Google.Common.Geometry;
using MongoDB.Driver.GeoJsonObjectModel;
using Ranger.Common;

namespace Ranger.Services.Geofences
{
    public static class Utilities
    {
        public static GeoJsonPoint<GeoJson2DGeographicCoordinates> GetPolygonCentroid(IEnumerable<LngLat> coordinates)
        {

            var latLngs = coordinates.Reverse().Select(c => S2LatLng.FromDegrees(c.Lat, c.Lng));
            var s2Loop = new S2Loop(latLngs.Select(_ => _.ToPoint()));
            s2Loop.Normalize();
            var s2Polygon = new S2Polygon(s2Loop);
            var s2AreaCentroid = s2Polygon.AreaAndCentroid;
            var area = s2AreaCentroid.Area * S2LatLng.EarthRadiusMeters * S2LatLng.EarthRadiusMeters;
            if (area < 10000)
            {
                throw new RangerException("Polygon geofences must enclose an area greater than 10,000 meters");
            }
            var centroid = new S2LatLng(s2Polygon.Centroid.Value);
            return GeoJson.Point<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(centroid.LngDegrees, centroid.LatDegrees));
        }

    }
}
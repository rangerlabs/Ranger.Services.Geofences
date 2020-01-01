using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.GeoJsonObjectModel;
using Ranger.Common;

namespace Ranger.Services.Geofences.Data
{
    public static class GeoJsonGeometryFactory
    {
        public static GeoJsonGeometry<GeoJson2DGeographicCoordinates> Factory(GeofenceShapeEnum shape, IEnumerable<LngLat> coordinates)
        {
            if (coordinates is null)
            {
                throw new ArgumentNullException($"{nameof(coordinates)} was null.");
            }
            switch (shape)
            {
                case GeofenceShapeEnum.Circle:
                    {
                        if (coordinates.Count() == 0 || coordinates.Count() > 1)
                        {
                            throw new ArgumentOutOfRangeException("Coordinates for circular geofences must contain exactly one point.");
                        }
                        return GeoJson.Point<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(coordinates.ElementAt(0).Lng, coordinates.ElementAt(0).Lat));
                    }
                case GeofenceShapeEnum.Polygon:
                    {
                        if (coordinates.Count() == 0 || coordinates.Count() < 3)
                        {
                            throw new ArgumentOutOfRangeException("Coordinates for polygon geofences must contain greater than three points.");
                        }
                        return GeoJson.Polygon<GeoJson2DGeographicCoordinates>(coordinates.Select(_ => new GeoJson2DGeographicCoordinates(_.Lng, _.Lat)).ToArray());
                    }
                default:
                    {
                        throw new ArgumentException("Invalid shape.");
                    }
            }
        }
    }
}
using System.Collections.ObjectModel;
using MongoDB.Driver.GeoJsonObjectModel;
using Ranger.Common;

namespace Ranger.Services.Geofences.Data
{
    public class RangerGeoJsonCoordinates : GeoJsonCoordinates
    {
        public RangerGeoJsonCoordinates(LngLat lngLat)
        {
            this.Values = new ReadOnlyCollection<double>(new[] { lngLat.Lng, lngLat.Lat });
        }

        public override ReadOnlyCollection<double> Values { get; }
    }
}
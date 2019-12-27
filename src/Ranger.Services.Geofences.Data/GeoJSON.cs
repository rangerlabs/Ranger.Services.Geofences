using System.Collections.Generic;

namespace Ranger.Services.Geofences.Data
{
    public class GeoJSON
    {
        public string type { get; set; }
        public IEnumerable<LngLat> coordinates { get; set; }
    }
}
namespace Ranger.Services.Geofences.Data
{
    public class LngLat
    {
        public LngLat(double lng, double lat)
        {
            this.Lng = lng;
            this.Lat = lat;
        }

        public double Lat { get; private set; }
        public double Lng { get; private set; }
    }
}
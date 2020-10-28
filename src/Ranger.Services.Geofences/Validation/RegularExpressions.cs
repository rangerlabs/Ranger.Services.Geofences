namespace Ranger.Services.Geofences
{
    public static class RegularExpressions
    {
        public static readonly string GEOFENCE_INTEGRATION_NAME = @"^[a-z0-9]+[a-z0-9\-]{1,126}[a-z0-9]{1}$";
    }
}
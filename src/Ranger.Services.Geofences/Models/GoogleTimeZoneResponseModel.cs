using System;

namespace Ranger.Services.Geofences
{
    public class GoogleTimeZoneResponseModel
    {
        public int dstOffset { get; set; }
        public int rawOffset { get; set; }
        public GoogleStatusEnum status { get; set; }
        public string timeZoneId { get; set; }
        public string timeZoneName { get; set; }
    }

    public enum GoogleStatusEnum
    {
        OK = 0,
        INVALID_REQUEST = 1,
        OVER_DAILY_LIMIT = 2,
        OVER_QUERY_LIMIT = 3,
        REQUEST_DENIED = 4,
        UNKNOWN_ERROR = 5,
        ZERO_RESULTS = 6
    }
}
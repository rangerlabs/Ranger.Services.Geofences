using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class UpsertGeofenceRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public UpsertGeofenceRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }
    }
}
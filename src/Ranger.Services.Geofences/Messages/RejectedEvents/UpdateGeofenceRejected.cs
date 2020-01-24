using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class UpdateGeofenceRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public UpdateGeofenceRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class CreateGeofenceRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public CreateGeofenceRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }
    }
}
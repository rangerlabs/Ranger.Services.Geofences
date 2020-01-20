using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class DeleteGeofenceRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public DeleteGeofenceRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }
    }
}
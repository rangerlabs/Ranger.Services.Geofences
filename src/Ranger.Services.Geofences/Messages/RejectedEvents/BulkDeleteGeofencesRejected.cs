using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class BulkDeleteGeofencesRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public BulkDeleteGeofencesRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }
    }
}
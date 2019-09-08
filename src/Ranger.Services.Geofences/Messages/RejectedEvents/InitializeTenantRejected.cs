using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class InitializeTenantRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public InitializeTenantRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }
    }
}
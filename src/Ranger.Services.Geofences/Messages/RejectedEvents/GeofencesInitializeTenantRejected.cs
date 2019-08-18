using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences {
    [MessageNamespaceAttribute ("geofences")]
    public class GeofencesInitializeTenantRejected : IRejectedEvent {
        public string Reason { get; }
        public string Code { get; }

        public GeofencesInitializeTenantRejected (string reason, string code) {
            this.Reason = reason;
            this.Code = code;
        }
    }
}
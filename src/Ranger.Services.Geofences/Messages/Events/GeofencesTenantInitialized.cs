using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences {
    [MessageNamespaceAttribute ("geofences")]
    public class GeofencesTenantInitialized : IEvent {

        public GeofencesTenantInitialized () { }
    }
}
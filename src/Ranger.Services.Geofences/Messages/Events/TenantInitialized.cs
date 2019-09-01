using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class TenantInitialized : IEvent
    {
        public TenantInitialized() { }
    }
}
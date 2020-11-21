using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class GeofencesBulkDeleted : IEvent
    {
        public string TenantId { get; }
        public string ExternalId { get; }

        public GeofencesBulkDeleted(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }

            this.TenantId = tenantId;
        }
    }
}
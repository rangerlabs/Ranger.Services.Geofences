using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class GeofenceDeleted : IEvent
    {
        public string TenantId { get; }
        public string ExternalId { get; }

        public GeofenceDeleted(string tenantId, string externalId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new System.ArgumentException($"{nameof(externalId)} was null or whitespace");
            }

            this.TenantId = tenantId;
            this.ExternalId = externalId;
        }
    }
}
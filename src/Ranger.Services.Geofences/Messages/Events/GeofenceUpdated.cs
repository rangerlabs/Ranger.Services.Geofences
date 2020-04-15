using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class GeofenceUpdated : IEvent
    {
        public string TenantId { get; }
        public string ExternalId { get; }
        public Guid Id { get; }

        public GeofenceUpdated(string tenantId, string externalId, Guid id)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new System.ArgumentException($"{nameof(externalId)} was null or whitespace.");
            }

            this.TenantId = tenantId;
            this.ExternalId = externalId;
            this.Id = id;
        }
    }
}
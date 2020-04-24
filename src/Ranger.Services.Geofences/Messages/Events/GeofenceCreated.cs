using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class GeofenceCreated : IEvent
    {
        public string TenantId { get; }
        public string ExternalId { get; }
        public Guid Id { get; }

        public GeofenceCreated(string tomain, string externalId, Guid id)
        {
            if (string.IsNullOrWhiteSpace(tomain))
            {
                throw new System.ArgumentException($"{nameof(tomain)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new System.ArgumentException($"{nameof(externalId)} was null or whitespace");
            }

            this.TenantId = tomain;
            this.ExternalId = externalId;
            this.Id = id;
        }
    }
}
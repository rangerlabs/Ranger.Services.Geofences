using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class GeofenceCreated : IEvent
    {
        public string Domain { get; }
        public string ExternalId { get; }
        public Guid Id { get; }

        public GeofenceCreated(string domain, string externalId, Guid id)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new System.ArgumentException($"{nameof(externalId)} was null or whitespace.");
            }

            this.Domain = domain;
            this.ExternalId = externalId;
            this.Id = id;
        }
    }
}
using System;
using System.Collections.Generic;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class GeofencesBulkDeleted : IEvent
    {
        public string TenantId { get; }
        public IEnumerable<Guid> DeletedIds { get; }

        public GeofencesBulkDeleted(string tenantId, IEnumerable<Guid> deletedIds)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }

            this.TenantId = tenantId;
            this.DeletedIds = deletedIds;
        }
    }
}
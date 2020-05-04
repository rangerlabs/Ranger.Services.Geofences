using System;
using System.Collections.Generic;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespace("geofences")]
    public class EnforceGeofenceResourceLimits : ICommand
    {
        public IEnumerable<(string tenantId, int limit, IEnumerable<Guid> remainingProjectIds)> TenantLimits { get; }

        public EnforceGeofenceResourceLimits(IEnumerable<(string tenantId, int limit, IEnumerable<Guid> remainingProjectIds)> tenantLimits)
        {
            TenantLimits = tenantLimits;
        }
    }
}
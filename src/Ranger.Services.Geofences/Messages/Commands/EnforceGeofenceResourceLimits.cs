using System.Collections.Generic;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespace("integrations")]
    public class EnforceGeofenceResourceLimits : ICommand
    {
        public IEnumerable<(string, int)> TenantLimits { get; }

        public EnforceGeofenceResourceLimits(IEnumerable<(string, int)> tenantLimits)
        {
            TenantLimits = tenantLimits;
        }
    }
}
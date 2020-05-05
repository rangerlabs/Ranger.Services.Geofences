using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences.Handlers
{
    public class EnforceGeofenceResourceLimitsHandler : ICommandHandler<EnforceGeofenceResourceLimits>
    {
        private readonly IGeofenceRepository geofenceRepo;
        private readonly ILogger<EnforceGeofenceResourceLimitsHandler> logger;

        public EnforceGeofenceResourceLimitsHandler(IGeofenceRepository geofenceRepo, ILogger<EnforceGeofenceResourceLimitsHandler> logger)
        {
            this.geofenceRepo = geofenceRepo;
            this.logger = logger;
        }

        public async Task HandleAsync(EnforceGeofenceResourceLimits message, ICorrelationContext context)
        {
            foreach (var tenantLimit in message.TenantLimits)
            {
                var geofenceCount = (int)(await geofenceRepo.GetAllActiveGeofencesCountAsync(tenantLimit.tenantId, tenantLimit.remainingProjectIds));
                if (geofenceCount > tenantLimit.limit)
                {
                    var exceededByCount = geofenceCount - tenantLimit.limit;
                    var geofences = await geofenceRepo.GetAllActiveGeofencesForProjectIdsAsync(tenantLimit.tenantId, tenantLimit.remainingProjectIds);
                    var geofencesToRemove = geofences.OrderByDescending(i => i.CreatedDate).Take(exceededByCount);
                    foreach (var geofenceToRemove in geofencesToRemove)
                    {
                        await geofenceRepo.DeleteGeofence(tenantLimit.tenantId, geofenceToRemove.ProjectId, geofenceToRemove.ExternalId, "SubscriptionEnforcer");
                    }
                }
            }
        }
    }
}
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences.Handlers
{
    public class EnforceGeofenceResourceLimitsHandler : ICommandHandler<EnforceGeofenceResourceLimits>
    {
        private readonly ILogger<EnforceGeofenceResourceLimitsHandler> logger;

        public EnforceGeofenceResourceLimitsHandler(ILogger<EnforceGeofenceResourceLimitsHandler> logger)
        {
            this.logger = logger;
        }

        public Task HandleAsync(EnforceGeofenceResourceLimits message, ICorrelationContext context)
        {
            logger.LogInformation("Enforcing geofence subscription limits");
            return Task.CompletedTask;
        }
    }
}
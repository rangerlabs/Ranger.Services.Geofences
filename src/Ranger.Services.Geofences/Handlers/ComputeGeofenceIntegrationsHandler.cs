using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences
{
    public class ComputeGeofenceIntegrationsHandler : ICommandHandler<ComputeGeofenceIntegrations>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<ComputeGeofenceIntegrationsHandler> logger;
        private readonly IGeofenceRepository geofenceRepository;

        public ComputeGeofenceIntegrationsHandler(IBusPublisher busPublisher, ILogger<ComputeGeofenceIntegrationsHandler> logger, IGeofenceRepository geofenceRepository)
        {
            this.logger = logger;
            this.geofenceRepository = geofenceRepository;
            this.busPublisher = busPublisher;
        }

        public async Task HandleAsync(ComputeGeofenceIntegrations message, ICorrelationContext context)
        {
            try
            {
                var geofences = await geofenceRepository.GetGeofencesAsync(message.TenantId, message.ProjectId, message.BreadcrumbGeofenceResults.Select(b => b.GeofenceId));
                var geofenceIntegrationResults = geofences
                    .Where(g =>
                        g.Enabled &&
                        g.Schedule.IsWithinSchedule(message.Breadcrumb.RecordedAt.ToUniversalTime(), logger) &&
                        IsConstructed(g, message.Breadcrumb.RecordedAt) &&
                        IsTriggerRequested(g, message.BreadcrumbGeofenceResults.Single(b => b.GeofenceId == g.Id).GeofenceEvent)
                    ).Select(g =>
                        new GeofenceIntegrationResult(
                            g.Id,
                            g.ExternalId,
                            g.Description,
                            g.Metadata,
                            g.IntegrationIds,
                            message.BreadcrumbGeofenceResults.Single(b => b.GeofenceId == g.Id).GeofenceEvent
                        )
                    );

                if (geofenceIntegrationResults.Any())
                {
                    busPublisher.Send(new ExecuteGeofenceIntegrations(message.TenantId, message.ProjectId, message.ProjectName, message.Environment, message.Breadcrumb, geofenceIntegrationResults), context);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to compute geofence integrations");
                throw;
            }
        }

        private bool IsConstructed(Geofence geofence, DateTime datetime)
        {
            if (geofence.LaunchDate <= datetime && datetime <= geofence.ExpirationDate)
            {
                logger.LogDebug("Determined recordedAt date was between LaunchDate and ExpirationDate");
                return true;
            }
            else
            {
                logger.LogDebug("Determined recordedAt date was NOT between LaunchDate and ExpirationDate");
                return false;
            }
        }
        private bool IsTriggerRequested(Geofence geofence, GeofenceEventEnum geofenceEvent)
        {
            var result = geofenceEvent switch
            {
                GeofenceEventEnum.ENTERED => geofence.OnEnter,
                GeofenceEventEnum.DWELLING => geofence.OnDwell,
                GeofenceEventEnum.EXITED => geofence.OnExit,
                _ => throw new ArgumentException("Invalid Event type")
            };
            logger.LogDebug("Determined Trigger request result to be {IsTriggered}", result);
            return result;
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
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
                var geofences = await geofenceRepository.GetGeofencesAsync(message.DatabaseUsername, message.ProjectId, message.BreadcrumbGeofenceResults.Select(b => b.GeofenceId));
                var geofenceIntegrationResults = geofences.Select(g =>
                    new GeofenceIntegrationResult(
                        g.Id,
                        g.ExternalId,
                        g.Description,
                        g.Metadata,
                        g.IntegrationIds,
                        message.BreadcrumbGeofenceResults.Single(b => b.GeofenceId == g.Id).GeofenceEvent)
                    );

                busPublisher.Send(new ExecuteGeofenceIntegrations(message.Domain, message.ProjectId, message.Environment, message.Breadcrumb, geofenceIntegrationResults), context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to compute intersecting geofences.");
            }
        }

        private bool IsConstructed(Geofence geofence, DateTime datetime) => geofence.LaunchDate <= datetime && datetime <= geofence.ExpirationDate;
    }
}
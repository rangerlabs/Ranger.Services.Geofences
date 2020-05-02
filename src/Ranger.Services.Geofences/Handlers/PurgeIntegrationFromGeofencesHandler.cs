using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.Services.Geofences.Data;
using Ranger.Services.Geofences.Messages.Commands;

namespace Ranger.Services.Geofences.Handlers
{
    public class PurgeIntegrationFromGeofencesHandler : ICommandHandler<PurgeIntegrationFromGeofences>
    {
        private readonly IGeofenceRepository geofenceRepository;
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<PurgeIntegrationFromGeofencesHandler> logger;
        public PurgeIntegrationFromGeofencesHandler(IBusPublisher busPublisher, IGeofenceRepository geofenceRepository, ILogger<PurgeIntegrationFromGeofencesHandler> logger)
        {
            this.logger = logger;
            this.busPublisher = busPublisher;
            this.geofenceRepository = geofenceRepository;
        }

        public async Task HandleAsync(PurgeIntegrationFromGeofences command, ICorrelationContext context)
        {
            try
            {
                await geofenceRepository.PurgeIntegrationFromAllGeofences(command.tenantId, command.projectId, command.integrationId);
                busPublisher.Publish(new IntegrationPurgedFromGeofences(), context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to purge integration with Id {Id} from geofences", command.integrationId);
                throw;
            }
        }
    }
}
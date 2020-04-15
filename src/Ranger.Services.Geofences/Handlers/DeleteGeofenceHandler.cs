using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences
{
    public class DeleteGeofenceHandler : ICommandHandler<DeleteGeofence>
    {
        private readonly IBusPublisher busPublisher;
        private readonly IGeofenceRepository repository;
        private readonly ILogger<DeleteGeofenceHandler> logger;

        public DeleteGeofenceHandler(IBusPublisher busPublisher, IGeofenceRepository repository, ILogger<DeleteGeofenceHandler> logger)
        {
            this.logger = logger;
            this.busPublisher = busPublisher;
            this.repository = repository;
        }

        public async Task HandleAsync(DeleteGeofence command, ICorrelationContext context)
        {
            try
            {
                await repository.DeleteGeofence(command.TenantId, command.ProjectId, command.ExternalId, command.CommandingUserEmailOrTokenPrefix);
                busPublisher.Publish(new GeofenceDeleted(command.TenantId, command.ExternalId), CorrelationContext.FromId(context.CorrelationContextId));
            }
            catch (RangerException)
            {
                throw;
            }
            catch (MongoException ex)
            {
                logger.LogError(ex, "Failed to delete geofence");
                throw new RangerException("An unspecified error occurred.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete geofence");
                throw new RangerException("An unspecified error occurred.", ex);
            }
        }
    }
}
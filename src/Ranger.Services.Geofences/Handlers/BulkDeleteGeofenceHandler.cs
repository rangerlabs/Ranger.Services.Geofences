using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences
{
    public class BulkDeleteGeofenceHandler : ICommandHandler<BulkDeleteGeofences>
    {
        private readonly IBusPublisher busPublisher;
        private readonly IGeofenceRepository repository;
        private readonly ILogger<BulkDeleteGeofenceHandler> logger;

        public BulkDeleteGeofenceHandler(IBusPublisher busPublisher, IGeofenceRepository repository, ILogger<BulkDeleteGeofenceHandler> logger)
        {
            this.logger = logger;
            this.busPublisher = busPublisher;
            this.repository = repository;
        }

        public async Task HandleAsync(BulkDeleteGeofences command, ICorrelationContext context)
        {
            try
            {
                await repository.BulkDeleteGeofence(command.TenantId, command.ProjectId, command.ExternalIds, command.CommandingUserEmailOrTokenPrefix);
                busPublisher.Publish(new GeofencesBulkDeleted(command.TenantId), CorrelationContext.FromId(context.CorrelationContextId));
            }
            catch (RangerException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred bulk deleting geofences");
                throw new RangerException($"An unexpected error occurred bulk deleting geofences.");
            }
        }
    }
}
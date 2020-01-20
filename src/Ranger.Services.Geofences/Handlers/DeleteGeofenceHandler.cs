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
        private readonly ITenantsClient tenantsClient;
        private readonly ILogger<DeleteGeofenceHandler> logger;

        public DeleteGeofenceHandler(IBusPublisher busPublisher, IGeofenceRepository repository, ITenantsClient tenantsClient, ILogger<DeleteGeofenceHandler> logger)
        {
            this.tenantsClient = tenantsClient;
            this.logger = logger;
            this.busPublisher = busPublisher;
            this.repository = repository;
        }

        public async Task HandleAsync(DeleteGeofence command, ICorrelationContext context)
        {
            ContextTenant tenant = null;
            try
            {
                tenant = await this.tenantsClient.GetTenantAsync<ContextTenant>(command.Domain);
            }
            catch (HttpClientException ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    throw new RangerException($"No tenant found for domain {command.Domain}.");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An exception occurred retrieving the ContextTenant object. Cannot construct the tenant specific repository.");
                throw;
            }

            try
            {
                await repository.DeleteGeofence(tenant.DatabaseUsername, command.ProjectId, command.ExternalId, command.CommandingUserEmailOrTokenPrefix);
            }
            catch (MongoException ex)
            {
                logger.LogError(ex, "Failed to delete geofence");
                throw new RangerException("Failed to delete geofence.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete geofence");
                throw new RangerException("Failed to delete geofence.", ex);
            }
            busPublisher.Publish(new GeofenceDeleted(command.Domain, command.ExternalId), CorrelationContext.FromId(context.CorrelationContextId));
        }
    }
}
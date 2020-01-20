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
    public class UpsertGeofenceHandler : ICommandHandler<UpsertGeofence>
    {
        private readonly IBusPublisher busPublisher;
        private readonly IGeofenceRepository repository;
        private readonly ITenantsClient tenantsClient;
        private readonly ILogger<UpsertGeofenceHandler> logger;

        public UpsertGeofenceHandler(IBusPublisher busPublisher, IGeofenceRepository repository, ITenantsClient tenantsClient, ILogger<UpsertGeofenceHandler> logger)
        {
            this.tenantsClient = tenantsClient;
            this.logger = logger;
            this.busPublisher = busPublisher;
            this.repository = repository;
        }

        public async Task HandleAsync(UpsertGeofence command, ICorrelationContext context)
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


            var geofence = new Geofence(Guid.NewGuid(), tenant.DatabaseUsername);
            geofence.ExternalId = command.ExternalId;
            geofence.ProjectId = command.ProjectId;
            geofence.Description = command.Description;
            geofence.Enabled = command.Enabled;
            geofence.ExpirationDate = command.ExpirationDate;
            geofence.GeoJsonGeometry = GeoJsonGeometryFactory.Factory(command.Shape, command.Coordinates);
            geofence.PolygonCentroid = command.Shape == GeofenceShapeEnum.Polygon ? Utilities.GetPolygonCentroid(command.Coordinates) : null;
            geofence.Labels = command.Labels;
            geofence.IntegrationIds = command.IntegrationIds;
            geofence.LaunchDate = command.LaunchDate;
            geofence.Metadata = command.Metadata;
            geofence.OnEnter = command.OnEnter;
            geofence.OnExit = command.OnExit;
            geofence.Radius = command.Radius;
            geofence.Schedule = command.Schedule;
            geofence.Shape = command.Shape;
            geofence.TimeZoneId = "Americas/New_York";

            try
            {
                await repository.UpsertGeofence(geofence, command.CommandingUserEmailOrTokenPrefix);
            }
            catch (MongoWriteException ex)
            {
                logger.LogError(ex, "Failed to upsert geofence");
                throw new RangerException("Failed to upsert geofence.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to upsert geofence");
                throw new RangerException("Failed to upsert geofence.", ex);
            }
            busPublisher.Publish(new GeofenceUpserted(command.Domain, command.ExternalId), CorrelationContext.FromId(context.CorrelationContextId));
        }
    }
}
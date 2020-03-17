using System;
using System.Collections.Generic;
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
    public class UpdateGeofenceHandler : ICommandHandler<UpdateGeofence>
    {
        private readonly IBusPublisher busPublisher;
        private readonly IGeofenceRepository repository;
        private readonly ITenantsClient tenantsClient;
        private readonly ILogger<UpdateGeofenceHandler> logger;

        public UpdateGeofenceHandler(IBusPublisher busPublisher, IGeofenceRepository repository, ITenantsClient tenantsClient, ILogger<UpdateGeofenceHandler> logger)
        {
            this.tenantsClient = tenantsClient;
            this.logger = logger;
            this.busPublisher = busPublisher;
            this.repository = repository;
        }

        public async Task HandleAsync(UpdateGeofence command, ICorrelationContext context)
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
            var geofence = new Geofence(command.Id, tenant.DatabaseUsername);
            geofence.ExternalId = command.ExternalId;
            geofence.ProjectId = command.ProjectId;
            geofence.Description = String.IsNullOrWhiteSpace(command.Description) ? "" : command.Description;
            geofence.Enabled = command.Enabled;
            geofence.ExpirationDate = command.ExpirationDate ?? DateTime.MaxValue;
            geofence.GeoJsonGeometry = GeoJsonGeometryFactory.Factory(command.Shape, command.Coordinates);
            geofence.PolygonCentroid = command.Shape == GeofenceShapeEnum.Polygon ? Utilities.GetPolygonCentroid(command.Coordinates) : null;
            geofence.Labels = command.Labels ?? new List<string>();
            geofence.IntegrationIds = command.IntegrationIds ?? new List<Guid>();
            geofence.LaunchDate = command.LaunchDate ?? DateTime.MinValue;
            geofence.Metadata = command.Metadata ?? new List<KeyValuePair<string, string>>();
            geofence.OnEnter = command.OnEnter;
            geofence.OnExit = command.OnExit;
            geofence.Radius = command.Radius;
            geofence.Schedule = command.Schedule ?? Schedule.FullUtcSchedule;
            geofence.Shape = command.Shape;

            try
            {
                await repository.UpdateGeofence(geofence, command.CommandingUserEmailOrTokenPrefix);
                busPublisher.Publish(new GeofenceUpdated(command.Domain, command.ExternalId, command.Id), CorrelationContext.FromId(context.CorrelationContextId));
            }
            catch (RangerException)
            {
                throw;
            }
            catch (MongoWriteException ex)
            {
                logger.LogError(ex, "Failed to upsert geofence");
                throw new RangerException("An unspecified error occurred.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to upsert geofence");
                throw new RangerException("An unspecified error occurred.", ex);
            }
        }
    }
}
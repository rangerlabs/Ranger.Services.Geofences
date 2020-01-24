using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Common.Geometry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences
{
    public class CreateGeofenceHandler : ICommandHandler<CreateGeofence>
    {
        private readonly IBusPublisher busPublisher;
        private readonly IGeofenceRepository repository;
        private readonly ITenantsClient tenantsClient;
        private readonly ILogger<CreateGeofenceHandler> logger;

        public CreateGeofenceHandler(IBusPublisher busPublisher, IGeofenceRepository repository, ITenantsClient tenantsClient, ILogger<CreateGeofenceHandler> logger)
        {
            this.tenantsClient = tenantsClient;
            this.logger = logger;
            this.busPublisher = busPublisher;
            this.repository = repository;
        }

        public async Task HandleAsync(CreateGeofence command, ICorrelationContext context)
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
                await repository.AddGeofence(geofence, command.CommandingUserEmailOrTokenPrefix);
                busPublisher.Publish(new GeofenceCreated(command.Domain, command.ExternalId, geofence.Id.ToString()), CorrelationContext.FromId(context.CorrelationContextId));
            }
            catch (RangerException)
            {
                throw;
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError != null && ex.WriteError.Code == 11000)
                {
                    throw new RangerException($"A geofence with the ExternalId '{command.ExternalId}' already exists.", ex);
                }
                throw new RangerException("An unspecified error occurred.", ex);
            }
            catch (Exception ex)
            {
                throw new RangerException("An unspecified error occurred.", ex);
            }
        }
    }
}
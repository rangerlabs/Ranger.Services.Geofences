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

            getCentroid(command.Coordinates);

            var geofence = new Geofence(Guid.NewGuid(), tenant.DatabaseUsername);
            geofence.ExternalId = command.ExternalId;
            geofence.ProjectId = command.ProjectId;
            geofence.Description = command.Description;
            geofence.Enabled = command.Enabled;
            geofence.ExpirationDate = command.ExpirationDate;
            geofence.GeoJsonGeometry = GeoJsonGeometryFactory.Factory(command.Shape, command.Coordinates);
            // geofence.PolygonCentroid = command.Shape == GeofenceShapeEnum.Polygon ? getCentroid(command.Coordinates) : null;
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
                await repository.AddGeofence(geofence);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError != null && ex.WriteError.Code == 11000)
                {
                    throw new RangerException($"A geofence with the ExternalId '{command.ExternalId}' already exists.", ex);
                }
                throw new RangerException("Failed to add geofence.", ex);
            }
            catch (Exception ex)
            {
                throw new RangerException("Failed to add geofence.", ex);
            }
            busPublisher.Publish(new GeofenceCreated(command.Domain, command.ExternalId), CorrelationContext.FromId(context.CorrelationContextId));
        }

        private void getCentroid(IEnumerable<LngLat> coordinates)
        {
            var s2PolygonBuilder = new S2PolygonBuilder();
            var latLngs = coordinates.Select(c => S2LatLng.FromDegrees(c.Lat, c.Lng));
            var s2Loop = new List<S2Loop>() { new S2Loop(latLngs.Select(_ => _.ToPoint())) };
            var unusedEdges = new List<S2Edge>();
            if (!s2PolygonBuilder.AssembleLoops(s2Loop, unusedEdges))
            {
                logger.LogInformation($"Unused edges ({unusedEdges.Select(_ => $"{{{_.Start}, {_.End}}}")}) were found in the polygon LngLat coordinates ({String.Join(";", coordinates.Select(_ => $"{{{_.Lng}, {_.Lat}}}"))}).");
                throw new RangerException("Invalid edges were found in the requested polygon coordinates.");
            }
            var s2Polygon = s2PolygonBuilder.AssemblePolygon();
            s2Polygon.
            var s2AreaCentroid = s2Polygon.AreaAndCentroid;
            if (s2AreaCentroid.Area < 10000)
            {
                throw new RangerException("Polygon geofences must enclose an area greater than 10,000 meters.");
            }
            logger.LogInformation($"Computed the centroid of the polygon to be {s2AreaCentroid.Centroid.Value.ToDegreesString()}");
        }
    }
}
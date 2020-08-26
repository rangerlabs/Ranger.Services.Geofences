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
        private readonly ILogger<UpdateGeofenceHandler> logger;

        public UpdateGeofenceHandler(IBusPublisher busPublisher, IGeofenceRepository repository, ILogger<UpdateGeofenceHandler> logger)
        {
            this.logger = logger;
            this.busPublisher = busPublisher;
            this.repository = repository;
        }

        public async Task HandleAsync(UpdateGeofence command, ICorrelationContext context)
        {

            var geofence = new Geofence(
                command.Id,
                command.TenantId,
                command.Shape,
                GeoJsonGeometryFactory.Factory(command.Shape, command.Coordinates),
                command.IntegrationIds ?? new List<Guid>(),
                command.Radius,
                command.ExternalId,
                command.ProjectId,
                String.IsNullOrWhiteSpace(command.Description) ? "" : command.Description,
                command.OnEnter,
                command.OnDwell,
                command.OnExit,
                command.Enabled,
                command.Shape == GeofenceShapeEnum.Polygon ? Utilities.GetPolygonCentroid(command.Coordinates) : null,
                command.Metadata ?? new List<KeyValuePair<string, string>>(),
                command.Labels ?? new List<string>(),
                command.ExpirationDate,
                command.LaunchDate,
                command.Schedule
            );

            try
            {
                await repository.UpdateGeofence(geofence, command.CommandingUserEmailOrTokenPrefix);
                busPublisher.Publish(new GeofenceUpdated(command.TenantId, command.ExternalId, command.Id), CorrelationContext.FromId(context.CorrelationContextId));
            }
            catch (RangerException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred updating geofence {ExternalId}", command.ExternalId);
                throw new RangerException($"An unexpected error occurred updating geofence '{command.ExternalId}'");
            }
        }
    }
}
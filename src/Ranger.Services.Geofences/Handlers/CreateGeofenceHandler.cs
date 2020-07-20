using System;
using System.Collections.Generic;
using System.Linq;
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
    public class CreateGeofenceHandler : ICommandHandler<CreateGeofence>
    {
        private readonly IBusPublisher busPublisher;
        private readonly IGeofenceRepository repository;
        private readonly SubscriptionsHttpClient subscriptionsHttpClient;
        private readonly ProjectsHttpClient projectsHttpClient;
        private readonly ILogger<CreateGeofenceHandler> logger;

        public CreateGeofenceHandler(IBusPublisher busPublisher, IGeofenceRepository repository, SubscriptionsHttpClient subscriptionsHttpClient, ProjectsHttpClient projectsHttpClient, ILogger<CreateGeofenceHandler> logger)
        {
            this.logger = logger;
            this.busPublisher = busPublisher;
            this.repository = repository;
            this.subscriptionsHttpClient = subscriptionsHttpClient;
            this.projectsHttpClient = projectsHttpClient;
        }

        public async Task HandleAsync(CreateGeofence command, ICorrelationContext context)
        {
            var limitsApiResponse = await subscriptionsHttpClient.GetSubscription<SubscriptionLimitDetails>(command.TenantId);
            var projectsApiResult = await projectsHttpClient.GetAllProjects<IEnumerable<Project>>(command.TenantId);
            var currentActiveGeofenceCount = await repository.GetAllActiveGeofencesCountAsync(command.TenantId, projectsApiResult.Result.Select(p => p.ProjectId));
            if (!limitsApiResponse.Result.Active)
            {
                throw new RangerException("Subscription is inactive");
            }
            if (currentActiveGeofenceCount >= limitsApiResponse.Result.Limit.Geofences)
            {
                throw new RangerException("Subscription limit met");
            }


            var geofence = new Geofence(Guid.NewGuid(), command.TenantId);
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
            geofence.OnDwell = command.OnDwell;
            geofence.OnExit = command.OnExit;
            geofence.Radius = command.Radius;
            geofence.Schedule = command.Schedule ?? Schedule.FullUtcSchedule;
            geofence.Shape = command.Shape;

            try
            {
                await repository.AddGeofence(geofence, command.CommandingUserEmailOrTokenPrefix);
                busPublisher.Publish(new GeofenceCreated(command.TenantId, command.ExternalId, geofence.Id), CorrelationContext.FromId(context.CorrelationContextId));
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError != null && ex.WriteError.Code == 11000)
                {
                    throw new RangerException($"A geofence with the ExternalId '{command.ExternalId}' already exists", ex);
                }
                logger.LogError(ex, "An unexpected error occurred creating geofence {ExternalId} with {Code}", command.ExternalId, ex.WriteError.Code);
                throw new RangerException($"An unexpected error occurred creating geofence '{command.ExternalId}'");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred creating geofence {ExternalId}", command.ExternalId);
                throw new RangerException($"An unexpected error occurred creating geofence '{command.ExternalId}'");
            }
        }
    }
}
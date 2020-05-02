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
        private readonly TenantsHttpClient tenantsClient;
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
            var limitsApiResponse = await subscriptionsHttpClient.GetLimitDetails<SubscriptionLimitDetails>(command.TenantId);
            var projectsApiResult = await projectsHttpClient.GetAllProjects<IEnumerable<Project>>(command.TenantId);
            var currentActiveGeofenceCount = await repository.GetAllActiveGeofencesCountAsync(command.TenantId, projectsApiResult.Result.Select(p => p.ProjectId));
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
                throw new RangerException("An unspecified error occurred", ex);
            }
            catch (Exception ex)
            {
                throw new RangerException("An unspecified error occurred", ex);
            }
        }
    }
}
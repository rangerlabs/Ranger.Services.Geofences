using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IntegrationsHttpClient integrationsHttpClient;
        private readonly ILogger<CreateGeofenceHandler> logger;

        public CreateGeofenceHandler(IBusPublisher busPublisher,
                                     IGeofenceRepository repository,
                                     SubscriptionsHttpClient subscriptionsHttpClient,
                                     ProjectsHttpClient projectsHttpClient,
                                     IntegrationsHttpClient integrationsHttpClient,
                                     ILogger<CreateGeofenceHandler> logger)
        {
            this.logger = logger;
            this.busPublisher = busPublisher;
            this.repository = repository;
            this.subscriptionsHttpClient = subscriptionsHttpClient;
            this.projectsHttpClient = projectsHttpClient;
            this.integrationsHttpClient = integrationsHttpClient;
        }

        public async Task HandleAsync(CreateGeofence command, ICorrelationContext context)
        {
            var limitsApiResponse = await subscriptionsHttpClient.GetSubscription<SubscriptionLimitDetails>(command.TenantId);
            var projectsApiResult = await projectsHttpClient.GetAllProjects<IEnumerable<Project>>(command.TenantId);
            var currentActiveGeofenceCount = await repository.GetAllActiveGeofencesCountAsync(command.TenantId, projectsApiResult.Result.Select(p => p.Id));
            if (!limitsApiResponse.Result.Active)
            {
                throw new RangerException("Subscription is inactive");
            }
            if (currentActiveGeofenceCount >= limitsApiResponse.Result.Limit.Geofences)
            {
                throw new RangerException("Subscription limit met");
            }

            IEnumerable<Guid> nonDefaultIntegrationIds = new List<Guid>();
            if (!(command.IntegrationIds is null) || command.IntegrationIds.Any())
            {
                var projectIntegrations = await integrationsHttpClient.GetAllIntegrationsByProjectId<IEnumerable<Integration>>(command.TenantId, command.ProjectId);
                var invalidIds = getInvalidIds(projectIntegrations.Result.Select(i => i.Id), command.IntegrationIds);
                if (invalidIds.Any())
                {
                    logger.LogDebug("The following IntegrationsIds are invalid for project {ProjectId} - {IntegrationIds}", command.ProjectId, String.Join(',', invalidIds));
                    throw new RangerException($"The following IntegrationsIds are invalid for this project: {String.Join(',', invalidIds)}");
                }
                nonDefaultIntegrationIds = removeDefaultIntegrationIds(projectIntegrations.Result, command.IntegrationIds);
            }

            var geofence = new Geofence(
                Guid.NewGuid(),
                command.TenantId,
                command.Shape,
                GeoJsonGeometryFactory.Factory(command.Shape, command.Coordinates),
                nonDefaultIntegrationIds,
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

        private IEnumerable<Guid> getInvalidIds(IEnumerable<Guid> projectIntegrations, IEnumerable<Guid> requestedIds)
        {
            foreach (var id in requestedIds)
            {
                if (!projectIntegrations.Contains(id))
                {
                    yield return id;
                }
            }
        }

        private IEnumerable<Guid> removeDefaultIntegrationIds(IEnumerable<Integration> projectIntegrations, IEnumerable<Guid> requestedIds)
        {
            foreach (var id in requestedIds)
            {
                if (!projectIntegrations.Single(i => i.Id.Equals(id)).IsDefault)
                {
                    yield return id;
                }
            }
        }
    }
}
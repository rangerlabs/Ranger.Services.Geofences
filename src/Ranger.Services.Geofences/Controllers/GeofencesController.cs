using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.GeoJsonObjectModel;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    public class GeofencesController : ControllerBase
    {
        private readonly IGeofenceRepository geofenceRepository;
        private readonly ITenantsHttpClient tenantsClient;
        private readonly ILogger<GeofencesController> logger;
        private readonly IProjectsHttpClient projectsHttpClient;

        public GeofencesController(IGeofenceRepository geofenceRepository, IProjectsHttpClient projectsHttpClient, ITenantsHttpClient tenantsClient, ILogger<GeofencesController> logger)
        {
            this.projectsHttpClient = projectsHttpClient;
            this.geofenceRepository = geofenceRepository;
            this.tenantsClient = tenantsClient;
            this.logger = logger;
        }

        ///<summary>
        /// Get all geofences for a specified project id
        ///</summary>
        ///<param name="tenantId">The tenant id to retrieve geofences for</param>
        ///<param name="projectId">The project id to retrieve geofences for</param>
        ///<param name="orderBy">The project id to retrieve geofences for</param>
        ///<param name="sortOrder">The project id to retrieve geofences for</param>
        ///<param name="page">The project id to retrieve geofences for</param>
        ///<param name="pageCount">The project id to retrieve geofences for</param>
        ///<param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/geofences/{tenantId}/{projectId}")]
        public async Task<ApiResponse> GetAllGeofences(
            string tenantId,
            Guid projectId,
            CancellationToken cancellationToken,
            [FromQuery] string orderBy = OrderByOptions.CreatedDate,
            [FromQuery] string sortOrder = GeofenceSortOrders.DescendingLowerInvariant,
            [FromQuery] int page = 1,
            [FromQuery] int pageCount = 100)
        {
            try
            {
                var geofences = await this.geofenceRepository.GetPaginatedGeofencesByProjectId(tenantId, projectId, orderBy, sortOrder, page, pageCount, cancellationToken);
                var geofenceResponse = new List<GeofenceResponseModel>();
                foreach (var geofence in geofences)
                {
                    geofenceResponse.Add(new GeofenceResponseModel
                    {
                        Id = geofence.Id,
                        Enabled = geofence.Enabled,
                        Description = geofence.Description,
                        ExpirationDate = geofence.ExpirationDate,
                        ExternalId = geofence.ExternalId,
                        Coordinates = getCoordinatesByShape(geofence.Shape, geofence.GeoJsonGeometry),
                        IntegrationIds = geofence.IntegrationIds,
                        Labels = geofence.Labels,
                        LaunchDate = geofence.LaunchDate,
                        Metadata = geofence.Metadata,
                        OnEnter = geofence.OnEnter,
                        OnDwell = geofence.OnDwell,
                        OnExit = geofence.OnExit,
                        ProjectId = geofence.ProjectId,
                        Radius = geofence.Radius,
                        Schedule = Schedule.IsUtcFullSchedule(geofence.Schedule) ? null : geofence.Schedule,
                        Shape = geofence.Shape,
                        CreatedDate = geofence.CreatedDate,
                        UpdatedDate = geofence.UpdatedDate
                    });
                }
                return new ApiResponse("Successfully retrieved geofences", geofenceResponse);
            }
            catch (Exception ex)
            {
                var message = "Failed to retrieve geofences";
                this.logger.LogError(ex, message);
                throw new ApiException(message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        ///<summary>
        /// Get all geofences that are in use by an active project
        ///</summary>
        ///<param name="tenantId">The tenant id to retrieve geofences for</param>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/geofences/{tenantId}/count")]
        public async Task<ApiResponse> GetAllGeofenceCount(string tenantId, CancellationToken cancellationToken)
        {
            var projects = await projectsHttpClient.GetAllProjects<IEnumerable<Project>>(tenantId, cancellationToken);
            try
            {
                var geofences = await geofenceRepository.GetAllActiveGeofencesCountAsync(tenantId, projects.Result.Select(p => p.Id), cancellationToken);
                return new ApiResponse("Successfully calculated active geofences", geofences);
            }
            catch (Exception ex)
            {
                var message = "Failed to retrieve active geofences count";
                this.logger.LogError(ex, message);
                throw new ApiException(message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        private IEnumerable<LngLat> getCoordinatesByShape(GeofenceShapeEnum shape, GeoJsonGeometry<GeoJson2DGeographicCoordinates> geoJsonGeometry)
        {
            switch (shape)
            {
                case GeofenceShapeEnum.Circle:
                    {
                        var point = (geoJsonGeometry as GeoJsonPoint<GeoJson2DGeographicCoordinates>);
                        return new LngLat[] { new LngLat(point.Coordinates.Longitude, point.Coordinates.Latitude) };
                    }
                case GeofenceShapeEnum.Polygon:
                    {
                        var count = (geoJsonGeometry as GeoJsonPolygon<GeoJson2DGeographicCoordinates>).Coordinates.Exterior.Positions.Count();
                        var points = (geoJsonGeometry as GeoJsonPolygon<GeoJson2DGeographicCoordinates>).Coordinates.Exterior.Positions.Take(count - 1).Select(_ => new LngLat(_.Longitude, _.Latitude));
                        return points;
                    }
                default:
                    return new LngLat[0];
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.GeoJsonObjectModel;
using Ranger.ApiUtilities;
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
        private readonly IValidator<GeofenceRequestParams> paramValidator;
        private readonly ILogger<GeofencesController> logger;
        private readonly IProjectsHttpClient projectsHttpClient;

        public GeofencesController(
            IGeofenceRepository geofenceRepository,
            IProjectsHttpClient projectsHttpClient,
            ITenantsHttpClient tenantsClient,
            IValidator<GeofenceRequestParams> paramValidator,
            ILogger<GeofencesController> logger)
        {
            this.projectsHttpClient = projectsHttpClient;
            this.geofenceRepository = geofenceRepository;
            this.tenantsClient = tenantsClient;
            this.paramValidator = paramValidator;
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
        /// <param name="bounds">The bounding rectangle to retrieve geofences within</param>
        ///<param name="cancellationToken"></param>
        /// <param name="externalId">The externalId to query for</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/geofences/{tenantId}/{projectId}")]
        public async Task<ApiResponse> GetAllGeofences(
            string tenantId,
            Guid projectId,
            CancellationToken cancellationToken,
            [FromQuery] string externalId = null,
            [FromQuery] string orderBy = OrderByOptions.CreatedDate,
            [FromQuery] string sortOrder = GeofenceSortOrders.DescendingLowerInvariant,
            [FromQuery] int page = 0,
            [FromQuery] int pageCount = 100,
            [FromQuery] [ModelBinder(typeof(SemicolonDelimitedLngLatArrayModelBinder))] IEnumerable<LngLat> bounds = null)
        {
            var validationResult = paramValidator.Validate(new GeofenceRequestParams(externalId, sortOrder, orderBy, page, pageCount, bounds), options => options.IncludeRuleSets("Get"));
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors.Select(f => new ValidationError(f.PropertyName, f.ErrorMessage));
                throw new ApiException(validationErrors);
            }

            try
            {
                IEnumerable<Geofence> geofences = default;
                if (!(externalId is null))
                {
                    logger.LogInformation("Retrieving geofence by externalId", externalId);
                    var geofence = await this.geofenceRepository.GetGeofenceOrDefaultAsync(tenantId, projectId, externalId, cancellationToken);
                    if (geofence is null)
                    {
                        throw new ApiException($"No geofence found for External Id: {externalId}", StatusCodes.Status404NotFound);
                    }

                    return new ApiResponse("Successfully retrieved geofence", GetResponseModel(geofence));
                }
                else if (!(bounds is null))
                {
                    logger.LogInformation("Retrieving bounded geofences", externalId);
                    geofences = await this.geofenceRepository.GetGeofencesByBoundingBox(tenantId, projectId, bounds, orderBy, sortOrder, cancellationToken);
                    if (geofences.Count() > 1000)
                    {
                        throw new ApiException("More than 1000 geofences exist within the requested bounds. Select a smaller area.");
                    }
                }
                else
                {
                    logger.LogInformation("Retrieving paginated geofences", externalId);
                    geofences = await this.geofenceRepository.GetPaginatedGeofencesByProjectId(tenantId, projectId, orderBy, sortOrder, page, pageCount, cancellationToken);
                }

                var geofenceResponse = new List<GeofenceResponseModel>();
                foreach(var geofence in geofences)
                {
                    geofenceResponse.Add(this.GetResponseModel(geofence));

                }
                return new ApiResponse("Successfully retrieved geofences", geofenceResponse);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = "Failed to retrieve geofences";
                this.logger.LogError(ex, message);
                throw new ApiException(message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
        
        private GeofenceResponseModel GetResponseModel(Geofence geofence)
        {
            return new GeofenceResponseModel
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
            };
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
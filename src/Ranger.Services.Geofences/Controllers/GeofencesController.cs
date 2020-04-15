using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
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
    public class GeofencesController : ControllerBase
    {
        private readonly IGeofenceRepository geofenceRepository;
        private readonly TenantsHttpClient tenantsClient;
        private readonly ILogger<GeofencesController> logger;

        public GeofencesController(IGeofenceRepository geofenceRepository, TenantsHttpClient tenantsClient, ILogger<GeofencesController> logger)
        {
            this.geofenceRepository = geofenceRepository;
            this.tenantsClient = tenantsClient;
            this.logger = logger;
        }

        ///<summary>
        /// Updates an existing project
        ///</summary>
        ///<param name="tenantId">The tenant id to retrieve geofences for</param>
        ///<param name="projectId">The project id to retrieve geofences for</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/{domain}/geofences/{projectId}")]
        public async Task<ApiResponse> GetAllGeofences(string tenantId, Guid projectId)
        {
            try
            {
                var geofences = await this.geofenceRepository.GetAllGeofencesByProjectId(tenantId, projectId);
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
                        Shape = geofence.Shape
                    });
                }
                return new ApiResponse("Successfully retrieved geofences", geofenceResponse);
            }
            catch (Exception ex)
            {
                var message = "An error occurred retrieving geofences";
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
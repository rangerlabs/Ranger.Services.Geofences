using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences.Handlers
{
    public class ComputeGeofenceIntersectionsHandler : ICommandHandler<ComputeGeofenceIntersections>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<ComputeGeofenceIntersectionsHandler> logger;
        private readonly IGeofenceRepository geofenceRepository;

        public ComputeGeofenceIntersectionsHandler(IBusPublisher busPublisher, ILogger<ComputeGeofenceIntersectionsHandler> logger, IGeofenceRepository geofenceRepository)
        {
            this.logger = logger;
            this.geofenceRepository = geofenceRepository;
            this.busPublisher = busPublisher;
        }

        public async Task HandleAsync(ComputeGeofenceIntersections message, ICorrelationContext context)
        {
            logger.LogDebug("Computing geofence intersections.");
            try
            {
                var geofences = await geofenceRepository.GetGeofencesContainingLocation(message.DatabaseUsername, message.ProjectId, message.Breadcrumb.Position, message.Breadcrumb.Accuracy);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to compute intersecting geofences.");
            }
            // if (message.Breadcrumb.Accuracy > 0)
            // {

            // }
            // else
            // {
            //     var point = GeoJson.Point(new GeoJson2DGeographicCoordinates(message.Breadcrumb.Longitude, message.Breadcrumb.Latitude));
            // }
            return;
        }
    }
}
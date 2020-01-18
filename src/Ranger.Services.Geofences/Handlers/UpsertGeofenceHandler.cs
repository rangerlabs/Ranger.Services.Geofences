using System.Threading.Tasks;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    public class UpsertGeofenceHandler : ICommandHandler<UpsertGeofence>
    {
        public Task HandleAsync(UpsertGeofence message, ICorrelationContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
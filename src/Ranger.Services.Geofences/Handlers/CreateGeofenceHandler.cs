using System.Threading.Tasks;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    public class CreateGeofenceHandler : ICommandHandler<CreateGeofence>
    {
        public CreateGeofenceHandler()
        {
        }

        public Task HandleAsync(CreateGeofence message, ICorrelationContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
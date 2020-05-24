using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespace("geofences")]
    public class IntegrationPurgedFromGeofences : IEvent
    { }
}
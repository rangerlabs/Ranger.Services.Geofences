using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences.Messages.RejectedEvents
{
    public class PurgeIntegrationFromGeofencesRejected : IRejectedEvent
    {
        public PurgeIntegrationFromGeofencesRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;

        }
        public string Reason { get; set; }
        public string Code { get; set; }
    }
}
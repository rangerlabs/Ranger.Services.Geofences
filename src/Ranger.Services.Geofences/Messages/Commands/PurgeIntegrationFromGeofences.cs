using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences.Messages.Commands
{
    [MessageNamespace("geofences")]
    public class PurgeIntegrationFromGeofences : ICommand
    {
        public Guid integrationId { get; }
        public Guid projectId { get; }
        public string tenantId { get; }

        public PurgeIntegrationFromGeofences(string tenantId, Guid projectId, Guid integrationId)
        {
            this.tenantId = tenantId;
            this.projectId = projectId;
            this.integrationId = integrationId;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class DeleteGeofence : ICommand
    {
        public DeleteGeofence(string commandingUserEmailOrTokenPrefix, string tenantId, string externalId, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new System.ArgumentException($"{nameof(externalId)} was null or whitespace");
            }

            this.CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;

            this.TenantId = tenantId;
            this.ExternalId = externalId;
            this.ProjectId = projectId;
        }

        public string CommandingUserEmailOrTokenPrefix { get; }
        public string TenantId { get; }
        public string ExternalId { get; }
        public Guid ProjectId { get; }
    }
}
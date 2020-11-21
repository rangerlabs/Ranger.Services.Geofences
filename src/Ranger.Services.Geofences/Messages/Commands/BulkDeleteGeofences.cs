using System;
using System.Collections.Generic;
using System.Linq;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class BulkDeleteGeofences : ICommand
    {
        public BulkDeleteGeofences(string commandingUserEmailOrTokenPrefix, string tenantId, IEnumerable<string> externalIds, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }
            if (externalIds is null || externalIds.Count() is 0)
            {
                throw new System.ArgumentException($"{nameof(externalIds)} must not be default or empty");
            }
            this.CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;
            this.TenantId = tenantId;
            this.ExternalIds = externalIds;
            this.ProjectId = projectId;
        }

        public string CommandingUserEmailOrTokenPrefix { get; }
        public string TenantId { get; }
        public IEnumerable<string> ExternalIds { get; }
        public Guid ProjectId { get; }
    }
}
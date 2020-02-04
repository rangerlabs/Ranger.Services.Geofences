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
        public DeleteGeofence(string commandingUserEmailOrTokenPrefix, string domain, string externalId, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new System.ArgumentException($"{nameof(externalId)} was null or whitespace.");
            }

            this.CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;

            this.Domain = domain;
            this.ExternalId = externalId;
            this.ProjectId = projectId;
        }

        public string CommandingUserEmailOrTokenPrefix { get; }
        public string Domain { get; }
        public string ExternalId { get; }
        public Guid ProjectId { get; }
    }
}
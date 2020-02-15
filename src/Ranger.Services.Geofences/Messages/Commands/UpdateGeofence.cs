using System;
using System.Collections.Generic;
using System.Linq;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class UpdateGeofence : ICommand
    {
        public UpdateGeofence(string commandingUserEmailOrTokenPrefix,
            string domain,
            Guid id,
            string externalId,
            Guid projectId,
            GeofenceShapeEnum shape,
            IEnumerable<LngLat> coordinates,
            IEnumerable<string> labels = null,
            IEnumerable<string> integrationIds = null,
            IEnumerable<KeyValuePair<string, string>> metadata = null,
            string description = null,
            int radius = 0,
            bool enabled = true,
            bool onEnter = true,
            bool onExit = true,
            DateTime? expirationDate = null,
            DateTime? launchDate = null,
            Schedule schedule = null)
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

            if (coordinates is null)
            {
                throw new System.ArgumentException($"{nameof(coordinates)} was null.");
            }
            if (coordinates.Count() == 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(coordinates)} must not be empty.");
            }

            this.CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;

            this.Coordinates = coordinates;
            this.Shape = shape;
            this.Radius = radius;

            this.Domain = domain;
            this.Id = id;
            this.ExternalId = externalId;
            this.ProjectId = projectId;
            this.Labels = labels ?? new List<string>();
            this.IntegrationIds = integrationIds ?? new List<string>();
            this.Metadata = metadata ?? new List<KeyValuePair<string, string>>();
            this.Description = string.IsNullOrWhiteSpace(description) ? "" : description;
            this.ExpirationDate = expirationDate ?? DateTime.MaxValue;
            this.LaunchDate = launchDate ?? DateTime.UtcNow;
            this.Schedule = schedule;
            this.Enabled = enabled;
            this.OnEnter = onEnter;
            this.OnExit = onExit;
        }

        public string CommandingUserEmailOrTokenPrefix { get; }
        public string Domain { get; }
        public Guid Id { get; }
        public string ExternalId { get; }
        public Guid ProjectId { get; }
        public IEnumerable<string> Labels { get; }
        public bool OnEnter { get; } = true;
        public bool OnExit { get; } = true;
        public bool Enabled { get; } = true;
        public string Description { get; }
        public IEnumerable<string> IntegrationIds { get; }
        public IEnumerable<LngLat> Coordinates { get; }
        public int Radius { get; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; }
        public GeofenceShapeEnum Shape { get; }
        public DateTime ExpirationDate { get; }
        public DateTime LaunchDate { get; }
        public Schedule Schedule { get; }
    }
}
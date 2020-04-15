using System;
using System.Collections.Generic;
using System.Linq;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class CreateGeofence : ICommand
    {
        public CreateGeofence(string commandingUserEmailOrTokenPrefix,
            string tenantId,
            string externalId,
            Guid projectId,
            GeofenceShapeEnum shape,
            IEnumerable<LngLat> coordinates,
            IEnumerable<string> labels = null,
            IEnumerable<Guid> integrationIds = null,
            IEnumerable<KeyValuePair<string, string>> metadata = null,
            string description = null,
            int radius = 0,
            bool enabled = true,
            bool onEnter = true,
            bool onDwell = true,
            bool onExit = true,
            DateTime? expirationDate = null,
            DateTime? launchDate = null,
            Schedule schedule = null)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace.");
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
            this.TenantId = tenantId;
            this.ExternalId = externalId;
            this.ProjectId = projectId;
            this.Labels = labels;
            this.IntegrationIds = integrationIds;
            this.Metadata = metadata;
            this.Description = description;
            this.ExpirationDate = expirationDate;
            this.LaunchDate = launchDate;
            this.Schedule = schedule;
            this.Enabled = enabled;
            this.OnEnter = onEnter;
            this.OnDwell = onDwell;
            this.OnExit = onExit;
        }

        public string CommandingUserEmailOrTokenPrefix { get; }
        public string TenantId { get; }
        public string ExternalId { get; }
        public Guid ProjectId { get; }
        public IEnumerable<string> Labels { get; }
        public bool OnEnter { get; } = true;
        public bool OnDwell { get; } = true;
        public bool OnExit { get; } = true;
        public bool Enabled { get; } = true;
        public string Description { get; }
        public IEnumerable<Guid> IntegrationIds { get; }
        public IEnumerable<LngLat> Coordinates { get; }
        public int Radius { get; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; }
        public GeofenceShapeEnum Shape { get; }
        public DateTime? ExpirationDate { get; }
        public DateTime? LaunchDate { get; }
        public Schedule Schedule { get; }
    }
}
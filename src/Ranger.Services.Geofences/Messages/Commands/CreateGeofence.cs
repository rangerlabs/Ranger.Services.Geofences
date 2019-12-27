using System;
using System.Collections.Generic;
using System.Linq;
using Ranger.RabbitMQ;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences
{
    public class CreateGeofence : ICommand
    {
        public CreateGeofence(string name, string projectName, GeofenceShapeEnum shape, IEnumerable<LngLat> coordinates, IEnumerable<string> labels = null, IEnumerable<string> integrationIds = null, IDictionary<string, object> metadata = null, string description = null, int radius = 0, bool enabled = true, bool onEnter = true, bool onExit = true, DateTime? expirationDate = null, DateTime? launchDate = null, Schedule schedule = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new System.ArgumentException($"{nameof(name)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(projectName))
            {
                throw new System.ArgumentException($"{nameof(projectName)} was null or whitespace.");
            }
            if (coordinates is null)
            {
                throw new System.ArgumentException($"{nameof(coordinates)} was null.");
            }
            if (coordinates.Count() == 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(coordinates)} must not be empty.");
            }

            this.Coordinates = coordinates;
            this.Shape = shape;
            this.Radius = Radius;

            this.Name = name;
            this.ProjectName = projectName;
            this.Labels = labels ?? new List<string>();
            this.IntegrationIds = integrationIds ?? new List<string>();
            this.Metadata = metadata ?? new Dictionary<string, object>();
            this.Description = string.IsNullOrWhiteSpace(description) ? "" : description;
            this.ExpirationDate = expirationDate ?? DateTime.MaxValue;
            this.LaunchDate = launchDate ?? DateTime.UtcNow;
            this.Schedule = schedule;
            this.Enabled = enabled;
            this.OnEnter = onEnter;
            this.OnExit = onExit;

        }

        public string Name { get; }
        public string ProjectName { get; }
        public IEnumerable<string> Labels { get; }
        public bool OnEnter { get; } = true;
        public bool OnExit { get; } = true;
        public bool Enabled { get; } = true;
        public string Description { get; }
        public IEnumerable<string> IntegrationIds { get; }
        public IEnumerable<LngLat> Coordinates { get; }
        public int Radius { get; }
        public IDictionary<string, object> Metadata { get; }
        public GeofenceShapeEnum Shape { get; }
        public DateTime ExpirationDate { get; set; }
        public DateTime LaunchDate { get; set; }
        public Schedule Schedule { get; set; }
    }
}
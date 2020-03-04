using System;
using Ranger.Common.SharedKernel;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespace("geofences")]
    public class ComputeGeofenceIntersections : ICommand
    {
        public string DatabaseUsername { get; set; }
        public Guid ProjectId { get; set; }
        public EnvironmentEnum Environment { get; set; }
        public Breadcrumb Breadcrumb { get; set; }

        public ComputeGeofenceIntersections(string databaseUsername, Guid projectId, EnvironmentEnum environment, Breadcrumb breadcrumb)
        {
            if (string.IsNullOrWhiteSpace(databaseUsername))
            {
                throw new ArgumentException($"{nameof(databaseUsername)} was null or whitespace.");
            }
            if (projectId.Equals(Guid.Empty))
            {
                throw new ArgumentException($"{nameof(projectId)} was an empty Guid.");
            }

            this.DatabaseUsername = databaseUsername;
            this.ProjectId = projectId;
            this.Environment = environment;
            this.Breadcrumb = breadcrumb ?? throw new ArgumentNullException(nameof(breadcrumb));
        }
    }
}
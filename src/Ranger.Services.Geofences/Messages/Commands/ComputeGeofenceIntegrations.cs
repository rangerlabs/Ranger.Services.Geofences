using System;
using System.Collections.Generic;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespace("geofences")]
    public class ComputeGeofenceIntegrations : ICommand
    {
        public string DatabaseUsername { get; }
        public string Domain { get; }
        public Guid ProjectId { get; }
        public EnvironmentEnum Environment { get; }
        public Common.Breadcrumb Breadcrumb { get; }
        public IEnumerable<BreadcrumbGeofenceResult> BreadcrumbGeofenceResults { get; }

        public ComputeGeofenceIntegrations(
            string databaseUsername,
            string domain,
            Guid projectId,
            EnvironmentEnum environment,
            Common.Breadcrumb breadcrumb,
            IEnumerable<BreadcrumbGeofenceResult> breadcrumbGeofenceResults
            )
        {
            if (string.IsNullOrWhiteSpace(databaseUsername))
            {
                throw new ArgumentException($"{nameof(databaseUsername)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (projectId.Equals(Guid.Empty))
            {
                throw new ArgumentException($"{nameof(projectId)} was an empty Guid.");
            }

            this.DatabaseUsername = databaseUsername;
            this.Domain = domain;
            this.ProjectId = projectId;
            this.Environment = environment;
            this.Breadcrumb = breadcrumb ?? throw new ArgumentNullException(nameof(breadcrumb));
            this.BreadcrumbGeofenceResults = breadcrumbGeofenceResults ?? throw new ArgumentNullException(nameof(breadcrumbGeofenceResults));
        }
    }

    public class BreadcrumbGeofenceResult
    {
        public Guid GeofenceId { get; set; }
        public GeofenceEventEnum GeofenceEvent { get; set; }
    }
}
using System;
using System.Collections.Generic;
using Ranger.Common;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.Services.Geofences
{
    [MessageNamespace("breadcrumbs")]
    public class ComputeGeofenceEvents : ICommand
    {
        public string TenantId { get; }
        public Guid ProjectId { get; }
        public string ProjectName { get; }
        public EnvironmentEnum Environment { get; }
        public Common.Breadcrumb Breadcrumb { get; }
        public IEnumerable<Guid> GeofenceIntersectionIds { get; }

        public ComputeGeofenceEvents(
            string tenantId,
            Guid projectId,
            string projectName,
            EnvironmentEnum environment,
            Common.Breadcrumb breadcrumb,
            IEnumerable<Guid> geofenceIntersectionIds
            )
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }

            if (projectId.Equals(Guid.Empty))
            {
                throw new ArgumentException($"{nameof(projectId)} was an empty Guid");
            }

            this.TenantId = tenantId;
            this.ProjectId = projectId;
            this.ProjectName = projectName;
            this.Environment = environment;
            this.Breadcrumb = breadcrumb ?? throw new ArgumentNullException(nameof(breadcrumb));
            this.GeofenceIntersectionIds = geofenceIntersectionIds;
        }
    }
}
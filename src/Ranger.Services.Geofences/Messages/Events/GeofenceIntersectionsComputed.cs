using System;
using System.Collections.Generic;
using System.Linq;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespace("geofences")]
    public class GeofenceIntersectionsComputed : IEvent
    {
        public string DatabaseUsername { get; }
        public string Domain { get; }
        public Guid ProjectId { get; }
        public EnvironmentEnum Environment { get; }
        public Breadcrumb Breadcrumb { get; }
        public IEnumerable<GeofenceIntegrationResult> GeofenceIntegrationResults { get; }

        public GeofenceIntersectionsComputed(
            string databaseUsername,
            string domain,
            Guid projectId,
            EnvironmentEnum environment,
            Breadcrumb breadcrumb,
            IEnumerable<GeofenceIntegrationResult> geofenceIntegrationResults
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
            this.GeofenceIntegrationResults = geofenceIntegrationResults;
        }
    }
    public class GeofenceIntegrationResult
    {
        public GeofenceIntegrationResult(Guid geofenceId, string geofenceExternalId, string geofenceDescription, IEnumerable<KeyValuePair<string, string>> geofenceMetadata, IEnumerable<Guid> integrationIds)
        {
            this.GeofenceId = geofenceId;
            this.GeofenceExternalId = geofenceExternalId;
            this.GeofenceDescription = geofenceDescription;
            this.GeofenceMetadata = geofenceMetadata;
            this.IntegrationIds = integrationIds;
        }

        public Guid GeofenceId { get; }
        public string GeofenceExternalId { get; }
        public string GeofenceDescription { get; }
        public IEnumerable<KeyValuePair<string, string>> GeofenceMetadata { get; }
        public IEnumerable<Guid> IntegrationIds { get; }
    }
}
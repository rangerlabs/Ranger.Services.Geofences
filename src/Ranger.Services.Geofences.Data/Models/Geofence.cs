using System;
using System.Collections.Generic;
using MongoDB.Driver.GeoJsonObjectModel;
using Ranger.Common;
using Ranger.Mongo;

namespace Ranger.Services.Geofences.Data
{
    public class Geofence : BaseMultitenantEntity
    {
        public GeofenceShapeEnum Shape { get; set; }
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> GeoJsonGeometry { get; set; }
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> PolygonCentroid { get; set; }
        public int Radius { get; set; }
        public string ExternalId { get; set; }
        public Guid ProjectId { get; set; }
        public string Description { get; set; }
        public IEnumerable<Guid> IntegrationIds { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
        public bool OnEnter { get; set; } = true;
        public bool OnDwell { get; set; } = true;
        public bool OnExit { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public IEnumerable<string> Labels { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime LaunchDate { get; set; }
        public Schedule Schedule { get; set; }

        public Geofence(
            Guid id,
            string tenantId,
            GeofenceShapeEnum shape,
            GeoJsonGeometry<GeoJson2DGeographicCoordinates> geoJsonGeometry,
            IEnumerable<Guid> integrationIds,
            int radius,
            string externalId,
            Guid projectId,
            string description,
            bool onEnter,
            bool onDwell,
            bool onExit,
            bool enabled,
            GeoJsonGeometry<GeoJson2DGeographicCoordinates> polygonCentroid = null,
            IEnumerable<KeyValuePair<string, string>> metadata = null,
            IEnumerable<string> labels = null,
            DateTime? expirationDate = null,
            DateTime? launchDate = null,
            Schedule schedule = null)
            : base(id, tenantId)
        {
            if (Guid.Equals(id, Guid.Empty))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be empty GUID");
            }
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"'{nameof(tenantId)}' cannot be null or whitespace", nameof(tenantId));
            }
            if (shape == GeofenceShapeEnum.Polygon && polygonCentroid is null)
            {
                throw new ArgumentException($"'{nameof(polygonCentroid)}' cannot be null for shape 'Polygon'");
            }
            if (shape == GeofenceShapeEnum.Circle && radius < 50)
            {
                throw new ArgumentException($"'{nameof(radius)}' cannot be less than 50 for shape 'Circle'");
            }
            if (geoJsonGeometry is null)
            {
                throw new ArgumentNullException(nameof(geoJsonGeometry));
            }
            if (string.IsNullOrEmpty(externalId))
            {
                throw new ArgumentException($"'{nameof(externalId)}' cannot be null or empty", nameof(externalId));
            }
            if (Guid.Equals(projectId, Guid.Empty))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be empty GUID");
            }

            ExternalId = externalId;
            ProjectId = projectId;
            Description = String.IsNullOrWhiteSpace(description) ? "" : description;
            Enabled = enabled;
            ExpirationDate = expirationDate ?? DateTime.MaxValue;
            GeoJsonGeometry = geoJsonGeometry;
            PolygonCentroid = polygonCentroid;
            Labels = labels ?? new List<string>();
            IntegrationIds = integrationIds ?? new List<Guid>();
            LaunchDate = launchDate ?? DateTime.MinValue;
            Metadata = metadata ?? new List<KeyValuePair<string, string>>();
            OnEnter = onEnter;
            OnDwell = onDwell;
            OnExit = onExit;
            Radius = radius;
            Schedule = schedule ?? Schedule.FullUtcSchedule;
            Shape = shape;
        }

        public void SetCreatedDate(DateTime createdDate)
        {
            if (createdDate >= this.UpdatedDate)
            {
                throw new ArgumentException($"'{nameof(createdDate)}' must be less than UpdateDate");
            }
            CreatedDate = createdDate;
        }

        public new void SetUpdatedDate() => base.SetUpdatedDate();
    }
}

using System;
using System.Collections.Generic;
using MongoDB.Driver.GeoJsonObjectModel;
using Ranger.Common;
using Ranger.Mongo;

namespace Ranger.Services.Geofences.Data
{
    public class Geofence : BaseMultitenantEntity
    {
        public Geofence(Guid id, string pgsqlDatabaseUsername) : base(id, pgsqlDatabaseUsername)
        { }

        public GeofenceShapeEnum Shape { get; set; }
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> GeoJsonGeometry { get; set; }
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> PolygonCentroid { get; set; }
        public int Radius { get; set; }

        public string ExternalId { get; set; }
        public Guid ProjectId { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Labels { get; set; }
        public IEnumerable<Guid> IntegrationIds { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
        public bool OnEnter { get; set; } = true;
        public bool OnDwell { get; set; } = true;
        public bool OnExit { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public DateTime ExpirationDate { get; set; }
        public DateTime LaunchDate { get; set; }
        public Schedule Schedule { get; set; }
    }
}

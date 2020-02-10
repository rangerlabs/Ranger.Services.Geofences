using System;
using System.Collections.Generic;
using MongoDB.Driver.GeoJsonObjectModel;
using Ranger.Common;
using Ranger.Mongo;

namespace Ranger.Services.Geofences.Data
{
    public class GeofenceResponseModel
    {
        public Guid Id { get; set; }
        public GeofenceShapeEnum Shape { get; set; }
        public IEnumerable<LngLat> Coordinates { get; set; }
        public int Radius { get; set; }

        public string ExternalId { get; set; }
        public Guid ProjectId { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Labels { get; set; }
        public IEnumerable<string> IntegrationIds { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public bool OnEnter { get; set; } = true;
        public bool OnExit { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public DateTime ExpirationDate { get; set; }
        public DateTime LaunchDate { get; set; }
        public Schedule Schedule { get; set; }
    }
}
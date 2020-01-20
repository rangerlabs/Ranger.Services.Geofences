using System;
using Ranger.Mongo;

namespace Ranger.Services.Geofences.Data
{
    public class GeofenceChangeLog : BaseMultitenantEntity
    {
        public GeofenceChangeLog(Guid id, string pgsqlDatabaseUsername) : base(id, pgsqlDatabaseUsername)
        { }

        public Guid GeofenceId { get; set; }
        public string ProjectId { get; set; }
        public string CommandingUserEmailOrTokenPrefix { get; set; }
        public string GeofenceDiff { get; set; }
        public string Event { get; set; }
    }
}
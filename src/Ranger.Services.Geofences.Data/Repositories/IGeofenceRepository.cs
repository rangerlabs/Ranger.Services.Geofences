using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ranger.Services.Geofences.Data
{
    public interface IGeofenceRepository
    {
        Task AddGeofence(Geofence geofence, string commandingUserEmailOrTokenPrefix);
        Task UpsertGeofence(Geofence geofence, string commandingUserEmailOrTokenPrefix);
        Task DeleteGeofence(string pgsqlDatabaseUsername, string projectId, string externalId, string commandingUserEmailOrTokenPrefix);
        Task<Geofence> GetGeofenceAsync(string pgsqlDatabaseUsername, string projectId, string externalId);
        Task<Geofence> GetGeofenceAsync(string pgsqlDatabaseUsername, string projectId, Guid geofenceId);
        Task<IEnumerable<GeofenceResponseModel>> GetAllGeofencesByProjectId(string pgsqlDatabaseUsername, string projectId);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ranger.Common;

namespace Ranger.Services.Geofences.Data
{
    public interface IGeofenceRepository
    {
        Task AddGeofence(Geofence geofence, string commandingUserEmailOrTokenPrefix);
        Task UpdateGeofence(Geofence geofence, string commandingUserEmailOrTokenPrefix);
        Task DeleteGeofence(string pgsqlDatabaseUsername, Guid projectId, string externalId, string commandingUserEmailOrTokenPrefix);
        Task<Geofence> GetGeofenceAsync(string pgsqlDatabaseUsername, Guid projectId, string externalId);
        Task<Geofence> GetGeofenceAsync(string pgsqlDatabaseUsername, Guid projectId, Guid geofenceId);
        Task<IEnumerable<Geofence>> GetGeofencesAsync(string pgsqlDatabaseUsername, Guid projectId, IEnumerable<Guid> geofenceIds);
        Task<IEnumerable<Geofence>> GetAllGeofencesByProjectId(string pgsqlDatabaseUsername, Guid projectId);
        Task<IEnumerable<Geofence>> GetGeofencesContainingLocation(string pgsqlDatabaseUsername, Guid projectId, LngLat lngLat, double accuracy = 0);
    }
}
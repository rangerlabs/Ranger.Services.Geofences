using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ranger.Services.Geofences.Data
{
    public interface IGeofenceRepository
    {
        Task AddGeofence(Geofence geofence, string commandingUserEmailOrTokenPrefix);
        Task UpsertGeofence(Geofence geofence, string commandingUserEmailOrTokenPrefix);
        Task<IEnumerable<GeofenceResponseModel>> GetAllGeofencesByProjectId(string pgsqlDatabaseUsername, string projectId);
    }
}
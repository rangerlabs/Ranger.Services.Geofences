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
        Task DeleteGeofence(string tenantId, Guid projectId, string externalId, string commandingUserEmailOrTokenPrefix);
        Task<Geofence> GetGeofenceAsync(string tenantId, Guid projectId, string externalId);
        Task<Geofence> GetGeofenceAsync(string tenantId, Guid projectId, Guid geofenceId);
        Task<IEnumerable<Geofence>> GetGeofencesAsync(string tenantId, Guid projectId, IEnumerable<Guid> geofenceIds);
        Task<IEnumerable<Geofence>> GetAllGeofencesByProjectId(string tenantId, Guid projectId);
        Task<IEnumerable<Geofence>> GetGeofencesContainingLocation(string tenantId, Guid projectId, LngLat lngLat, double accuracy = 0);
    }
}
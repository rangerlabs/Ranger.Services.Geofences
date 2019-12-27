using System.Threading.Tasks;

namespace Ranger.Services.Geofences.Data
{
    public interface IGeofenceRepository
    {
        Task AddGeofence(Geofence geofence);
    }
}
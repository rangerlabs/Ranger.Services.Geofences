using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Ranger.Common;

namespace Ranger.Services.Geofences.Data
{
    public interface IGeofenceRepository
    {
        IMongoDatabase Database {get;}
        Task AddGeofence(Geofence geofence, string commandingUserEmailOrTokenPrefix);
        Task UpdateGeofence(Geofence geofence, string commandingUserEmailOrTokenPrefix);
        Task DeleteGeofence(string tenantId, Guid projectId, string externalId, string commandingUserEmailOrTokenPrefix);
        Task<Geofence> GetGeofenceOrDefaultAsync(string tenantId, Guid projectId, string externalId, CancellationToken cancellationToken = default(CancellationToken));
        Task<Geofence> GetGeofenceAsync(string tenantId, Guid projectId, Guid geofenceId, CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<Geofence>> SearchGeofencesAsync(string tenantId, Guid projectId, string search, CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<Geofence>> GetGeofencesAsync(string tenantId, Guid projectId, IEnumerable<Guid> geofenceIds, CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<Geofence>> GetAllGeofencesByProjectId(string tenantId, Guid projectId, CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<Geofence>> GetGeofencesContainingLocation(string tenantId, Guid projectId, LngLat lngLat, double accuracy = 0, CancellationToken cancellationToken = default(CancellationToken));
        Task PurgeIntegrationFromAllGeofences(string tenantId, Guid projectId, Guid integrationId);
        Task<long> GetAllActiveGeofencesCountAsync(string tenantId, IEnumerable<Guid> projectIds, CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<Geofence>> GetAllActiveGeofencesForProjectIdsAsync(string tenantId, IEnumerable<Guid> projectIds, CancellationToken cancellationToken = default(CancellationToken));
        Task<long> GetGeofencesCountForProjectAsync(string tenantId, Guid projectId, CancellationToken cancellationToken = default(CancellationToken));
        Task<(IEnumerable<Geofence> geofences, long totalCount)> GetPaginatedGeofencesByProjectId(string tenantId, Guid projectId, string orderBy = OrderByOptions.CreatedDateLowerInvariant, string sortOrder = GeofenceSortOrders.DescendingLowerInvariant, int page = 1, int pageCount = 100, CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<Geofence>> GetGeofencesByBoundingBox(string tenantId, Guid projectId, IEnumerable<LngLat> boundingBox, string orderBy = OrderByOptions.CreatedDateLowerInvariant, string sortOrder = GeofenceSortOrders.Descending, CancellationToken cancellationToken = default(CancellationToken));
    }
}
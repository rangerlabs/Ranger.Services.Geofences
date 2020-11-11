using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.Services.Geofences
{
    public class GeofenceRequestParams
    {
        public GeofenceRequestParams(string externalId, string geofenceSortOrder, string orderByOption, int page, int pageCount, IEnumerable<LngLat> bounds = null)
        {
            this.ExternalId = externalId;
            this.GeofenceSortOrder = geofenceSortOrder;
            this.OrderByOption = orderByOption;
            this.Page = page;
            this.PageCount = pageCount;
            this.Bounds = bounds;
        }

        public string ExternalId { get; set; }
        public string GeofenceSortOrder { get; set; }
        public string OrderByOption { get; set; }
        public int Page { get; set; }
        public int PageCount { get; set; }
        public IEnumerable<LngLat> Bounds { get; set; }
    }
}
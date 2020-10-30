namespace Ranger.Services.Geofences
{
    public class GeofenceRequestParams
    {
        public GeofenceRequestParams(string geofenceSortOrder, string orderByOption, int page, int pageCount)
        {
            this.GeofenceSortOrder = geofenceSortOrder;
            this.OrderByOption = orderByOption;
            this.Page = page;
            this.PageCount = pageCount;
        }

        public string GeofenceSortOrder { get; set;}
        public string OrderByOption { get; set;}
        public int Page { get; set;}
        public int PageCount { get; set; }
    }
}
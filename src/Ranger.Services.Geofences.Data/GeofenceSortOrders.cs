using System;

namespace Ranger.Services.Geofences
{
    public static class GeofenceSortOrders
    {
        public const string Ascending = "1";
        public const string AscendingLowerInvariant = "asc";
        public const string Descending = "-1";
        public const string DescendingLowerInvariant = "desc";


        public static string SortOrderMap(string sortOrder)
        {
            sortOrder = sortOrder.ToLowerInvariant();
            return sortOrder switch
            {
                GeofenceSortOrders.AscendingLowerInvariant => GeofenceSortOrders.Ascending,
                GeofenceSortOrders.DescendingLowerInvariant => GeofenceSortOrders.Descending,
               _ => throw new InvalidOperationException()
            };
        }
    }
}
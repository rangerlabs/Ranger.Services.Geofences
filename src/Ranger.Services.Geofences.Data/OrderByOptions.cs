using System;

namespace Ranger.Services.Geofences
{
    public static class OrderByOptions
    {
        public const string ExternalId = "ExternalId";
        public const string ExternalIdLowerInvariant = "externalid";
        public const string Shape = "Shape";
        public const string ShapeLowerInvariant = "shape";
        public const string Enabled = "Enabled";
        public const string EnabledLowerInvariant = "enabled";
        public const string CreatedDate = "CreatedDate";
        public const string CreatedDateLowerInvariant = "createddate";
        public const string UpdatedDate = "UpdatedDate";
        public const string UpdatedDateLowerInvariant = "updateddate";

        public static string NamingMap(string name)
        {
            name = name.ToLowerInvariant();
            return name switch
            {
                OrderByOptions.ExternalIdLowerInvariant => OrderByOptions.ExternalId,
                OrderByOptions.ShapeLowerInvariant=> OrderByOptions.Shape,
                OrderByOptions.EnabledLowerInvariant => OrderByOptions.Enabled,
                OrderByOptions.CreatedDateLowerInvariant => OrderByOptions.CreatedDate,
                OrderByOptions.UpdatedDateLowerInvariant => OrderByOptions.UpdatedDate,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
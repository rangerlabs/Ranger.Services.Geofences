using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ranger.Services.Geofences.Tests
{
    public static class Extensions
    {
        public static T GetRequiredServiceForTest<T>(this IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                return services.GetRequiredService<T>();
            };
        }
    }
}
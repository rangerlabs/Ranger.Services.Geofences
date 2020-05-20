using System;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ranger.Monitoring.Logging;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var host = CreateWebHostBuilder(config["serverBindingUrl"], args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<IGeofencesDbContextInitializer>();
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                dbInitializer.Migrate();
            }
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string serverBindingUrl, string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseUrls(serverBindingUrl)
            .UseLogging()
            .UseStartup<Startup>()
            .ConfigureServices(services => services.AddAutofac());
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ranger.Mongo;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences.Tests.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(Environments.Production);

            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables();
            });

            builder.ConfigureServices(services =>
            {
                var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

                services.AddDbContext<GeofencesDbContext>(options =>
                    {
                        options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
                    });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var logger = scope.ServiceProvider.GetService<ILogger<CustomWebApplicationFactory>>();
                    var context = scope.ServiceProvider.GetRequiredService<GeofencesDbContext>();
                    context.Database.Migrate();
                }
            });
        }
    }
}
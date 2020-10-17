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
                services.AddAutofac();

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

        protected override IHost CreateHost(IHostBuilder builder) {
            builder.UseServiceProviderFactory(new CustomServiceProviderFactory());
            return base.CreateHost(builder);
        }

        /// <summary>
        /// Based upon https://github.com/dotnet/aspnetcore/issues/14907#issuecomment-620750841 - only necessary because of an issue in ASP.NET Core
        /// </summary>
        public class CustomServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
        {
            private AutofacServiceProviderFactory _wrapped;
            private IServiceCollection _services;

            public CustomServiceProviderFactory()
            {
                _wrapped = new AutofacServiceProviderFactory();
            }

            public ContainerBuilder CreateBuilder(IServiceCollection services)
            {
                // Store the services for later.
                _services = services;

                return _wrapped.CreateBuilder(services);
            }

            public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
            {
                var sp = _services.BuildServiceProvider();
#pragma warning disable CS0612 // Type or member is obsolete
                var filters = sp.GetRequiredService<IEnumerable<IStartupConfigureContainerFilter<ContainerBuilder>>>();
#pragma warning restore CS0612 // Type or member is obsolete

                foreach (var filter in filters)
                {
                    filter.ConfigureContainer(b => { })(containerBuilder);
                }

                return _wrapped.CreateServiceProvider(containerBuilder);
            }        
        }
    }
}
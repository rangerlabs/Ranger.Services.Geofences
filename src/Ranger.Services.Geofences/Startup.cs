using System;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JsonDiffPatchDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Ranger.ApiUtilities;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Mongo;
using Ranger.Monitoring.HealthChecks;
using Ranger.RabbitMQ;
using Ranger.Redis;
using Ranger.Services.Geofences.Data;
using Ranger.Services.Geofences.Messages.Commands;
using Ranger.Services.Geofences.Messages.RejectedEvents;

namespace Ranger.Services.Geofences
{
    public class Startup
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration configuration;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.Environment = environment;
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
                {
                    options.EnableEndpointRouting = false;
                    options.Filters.Add<OperationCanceledExceptionFilter>();
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                });

            services.AddRangerApiVersioning();
            services.ConfigureAutoWrapperModelStateResponseFactory();

            var identityAuthority = configuration["httpClient:identityAuthority"];
            services.AddPollyPolicyRegistry();
            services.AddTenantsHttpClient("http://tenants:8082", identityAuthority, "tenantsApi", "cKprgh9wYKWcsm");
            services.AddProjectsHttpClient("http://projects:8086", identityAuthority, "projectsApi", "usGwT8Qsp4La2");
            services.AddSubscriptionsHttpClient("http://subscriptions:8089", identityAuthority, "subscriptionsApi", "4T3SXqXaD6GyGHn4RY");
            services.AddIntegrationsHttpClient("http://integrations:8087", identityAuthority, "integrationsApi", "6HyhzSoSHvxTG");

            services.AddDbContext<GeofencesDbContext>(options =>
            {
                options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            });

            services.AddTransient<IGeofencesDbContextInitializer, GeofencesDbContextInitializer>();
            services.AddTransient<ILoginRoleRepository<GeofencesDbContext>, LoginRoleRepository<GeofencesDbContext>>();
            services.AddTransient<IGeofenceRepository, GeofenceRepository>();
            services.AddSingleton<IMongoDbSeeder, GeofenceSeeder>();

            services.AddRedis(configuration["redis:ConnectionString"], out _);

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "geofencesApi";
                    options.RequireHttpsMetadata = false;
                });

            // Workaround for MAC validation issues on MacOS
            if (configuration.IsIntegrationTesting())
            {
                services.AddDataProtection()
                    .SetApplicationName("Geofences")
                    .PersistKeysToDbContext<GeofencesDbContext>();
            }
            else
            {
                if (Environment.IsDevelopment())
                {
                    services.AddSwaggerGen("Geofences API", "v1");
                }
                services.AddDataProtection()
                    .SetApplicationName("Geofences")
                    .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                    .UnprotectKeysWithAnyCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                    .PersistKeysToDbContext<GeofencesDbContext>();
            }
            services.AddLiveHealthCheck();
            services.AddEntityFrameworkHealthCheck<GeofencesDbContext>();
            services.AddDockerImageTagHealthCheck();
            services.AddRabbitMQHealthCheck();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddRabbitMqWithOutbox<Startup, GeofencesDbContext>();
            builder.AddMongo();
            builder.RegisterInstance<JsonDiffPatch>(new JsonDiffPatch());
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<Startup>();

            if (Environment.IsDevelopment())
            {
                app.UseSwagger("v1", "Geofences API");
            }
            app.UseAutoWrapper();
            app.UseUnhandedExceptionLogger();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks();
                endpoints.MapLiveTagHealthCheck();
                endpoints.MapEfCoreTagHealthCheck();
                endpoints.MapDockerImageTagHealthCheck();
                endpoints.MapRabbitMQHealthCheck();
            });
            app.UseRabbitMQ()
                .SubscribeCommandWithHandler<CreateGeofence>((c, e) =>
                    new CreateGeofenceRejected(e.Message, "")
                )
                .SubscribeCommandWithHandler<UpdateGeofence>((c, e) =>
                    new UpdateGeofenceRejected(e.Message, "")
                )
                .SubscribeCommandWithHandler<DeleteGeofence>((c, e) =>
                    new DeleteGeofenceRejected(e.Message, "")
                )
                .SubscribeCommandWithHandler<PurgeIntegrationFromGeofences>((c, e) =>
                    new PurgeIntegrationFromGeofencesRejected(e.Message, "")
                )
                .SubscribeCommandWithHandler<ComputeGeofenceIntegrations>()
                .SubscribeCommandWithHandler<ComputeGeofenceIntersections>()
                .SubscribeCommandWithHandler<EnforceGeofenceResourceLimits>();

            this.InitializeMongoDb(app, logger);
        }

        private void InitializeMongoDb(IApplicationBuilder app, ILogger<Startup> logger)
        {
            logger.LogInformation("Initializing MongoDB");
            var mongoInitializer = app.ApplicationServices.GetService<IMongoDbInitializer>();
            mongoInitializer.Initialize();
        }
    }
}
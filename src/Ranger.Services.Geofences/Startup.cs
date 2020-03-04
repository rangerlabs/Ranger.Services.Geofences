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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Mongo;
using Ranger.RabbitMQ;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences
{
    public class Startup
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration configuration;
        private ILoggerFactory loggerFactory;
        private IBusSubscriber busSubscriber;

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
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("geofencesApi", policyBuilder =>
                    {
                        policyBuilder.RequireScope("geofencesApi");
                    });
            });

            services.AddSingleton<ITenantsClient, TenantsClient>(provider =>
            {
                return new TenantsClient("http://tenants:8082", loggerFactory.CreateLogger<TenantsClient>());
            });

            services.AddEntityFrameworkNpgsql().AddDbContext<GeofencesDbContext>(options =>
            {
                options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            },
                ServiceLifetime.Transient
            );

            services.AddTransient<IGeofencesDbContextInitializer, GeofencesDbContextInitializer>();
            services.AddTransient<ILoginRoleRepository<GeofencesDbContext>, LoginRoleRepository<GeofencesDbContext>>();
            services.AddTransient<IGeofenceRepository, GeofenceRepository>();
            services.AddSingleton<IMongoDbSeeder, GeofenceSeeder>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "geofencesApi";

                    options.RequireHttpsMetadata = false;
                });

            services.AddDataProtection()
                .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .PersistKeysToDbContext<GeofencesDbContext>();

        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddRabbitMq();
            builder.AddMongo();
            builder.RegisterInstance<JsonDiffPatch>(new JsonDiffPatch());
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            var logger = loggerFactory.CreateLogger<Startup>();
            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            app.UseRouting();
            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            this.busSubscriber = app.UseRabbitMQ()
                .SubscribeCommand<CreateGeofence>((c, e) =>
                    new CreateGeofenceRejected(e.Message, "")
                )
                .SubscribeCommand<UpdateGeofence>((c, e) =>
                    new UpdateGeofenceRejected(e.Message, "")
                )
                .SubscribeCommand<DeleteGeofence>((c, e) =>
                    new DeleteGeofenceRejected(e.Message, "")
                )
                .SubscribeCommand<ComputeGeofenceIntersections>();
            ;


            this.InitializeMongoDb(app, logger);
        }

        private void OnShutdown()
        {
            this.busSubscriber.Dispose();
        }

        private void InitializeMongoDb(IApplicationBuilder app, ILogger<Startup> logger)
        {
            logger.LogInformation("Initializing MongoDB.");
            var mongoInitializer = app.ApplicationServices.GetService<IMongoDbInitializer>();
            mongoInitializer.Initialize();
            logger.LogInformation("MongoDB Initialized.");
        }
    }
}
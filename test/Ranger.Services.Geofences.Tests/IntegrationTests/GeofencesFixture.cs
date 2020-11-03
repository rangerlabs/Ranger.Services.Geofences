using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Moq;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.RabbitMQ.BusSubscriber;
using Ranger.Services.Geofences.Data;
using Xunit;

namespace Ranger.Services.Geofences.Tests.IntegrationTests
{
    public class GeofencesFixture 
    {
        private IMongoCollection<Geofence> geofenceCollection;
        private IMongoCollection<GeofenceChangeLog> geofenceChangeLogCollection;
        private IGeofenceRepository geofencesRepository;
        private IBusPublisher busPublisher;
        private IBusSubscriber busSubscriber;
        public HttpClient httpClient;

        public GeofencesFixture(CustomWebApplicationFactory factory)
        {
            this.busPublisher = factory.Services.GetService(typeof(IBusPublisher)) as IBusPublisher;
            this.busSubscriber = factory.Services.GetService(typeof(IBusSubscriber)) as IBusSubscriber;
            this.geofencesRepository = factory.Services.GetService(typeof(IGeofenceRepository)) as IGeofenceRepository;
            this.SeedMongoDb(this.geofencesRepository);
            this.httpClient = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test", options => {});
                });
            }).CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            httpClient.DefaultRequestHeaders.Add("api-version", "1.0");
        }

        public void SeedMongoDb(IGeofenceRepository geofenceRepository)
        {
            this.geofenceCollection = geofenceRepository.Database.GetCollection<Geofence>(CollectionNames.GeofenceCollection);
            this.geofenceChangeLogCollection = geofenceRepository.Database.GetCollection<GeofenceChangeLog>(CollectionNames.GeofenceChangeLogCollection);
            Seed.SeedGeofences(geofenceRepository);
        }

        public void ClearMongoDb()
        {
            geofenceCollection.DeleteMany((_) => true);
            geofenceChangeLogCollection.DeleteMany((_) => true);
        }
    }
}
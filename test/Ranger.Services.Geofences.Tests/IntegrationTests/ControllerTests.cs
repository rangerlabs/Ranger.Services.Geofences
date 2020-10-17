using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Ranger.InternalHttpClient;
using Ranger.Mongo;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.RabbitMQ.BusSubscriber;
using Ranger.Services.Geofences.Data;
using Shouldly;
using Xunit;

namespace Ranger.Services.Geofences.Tests.IntegrationTests
{
    public class ControllerTests : IClassFixture<CustomWebApplicationFactory>, IClassFixture<GeofencesFixture>
    {
        private readonly IBusPublisher busPublisher;
        private readonly IBusSubscriber busSubscriber;
        private readonly IGeofenceRepository geofencesRepository;
        private readonly CustomWebApplicationFactory _factory;
        private readonly GeofencesFixture _fixture;

        public ControllerTests(CustomWebApplicationFactory factory, GeofencesFixture fixture)
        {
            _factory = factory;
            this._fixture = fixture;
            this.busPublisher = factory.Services.GetService(typeof(IBusPublisher)) as IBusPublisher;
            this.busSubscriber = factory.Services.GetService(typeof(IBusSubscriber)) as IBusSubscriber;
            this.geofencesRepository = factory.Services.GetService(typeof(IGeofenceRepository)) as IGeofenceRepository;
            this._fixture.SeedMongoDb(this.geofencesRepository);
        }

        [Fact]
        public async Task GetAllGeofences_ReturnsAllGeofences_ForValidTenantAndProjectIds()
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test", options => {});
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await client.GetAsync($"/geofences/{TenantOneGeofences.TenantId}/{TenantOneGeofences.ProjectId1}");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);

            result.StatusCode.ShouldBe(StatusCodes.Status200OK);
            result.Result.Count().ShouldBe(TenantOneGeofences.Project1Geofences().Count());
        }
    }
}
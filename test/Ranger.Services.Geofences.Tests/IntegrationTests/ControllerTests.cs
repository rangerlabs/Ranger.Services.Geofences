using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Ranger.InternalHttpClient;
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
        public async Task GetAllGeofences_Returns100PaginatedGeofencesSortedByCreatedDateDescending_ForValidTenantAndProjectIds()
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

            var response = await client.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);

            for (int i = 0; i < result.Result.Count(); i++)
            {
                if (i != 0)
                {
                    var previousGeofence = result.Result.ElementAt(i - 1);
                    var currentGeofence = result.Result.ElementAt(i);
                    previousGeofence.CreatedDate.ShouldBeGreaterThan(currentGeofence.CreatedDate);
                }
            }
        }

        [Fact]
        public async Task GetAllGeofences_Returns100PaginatedGeofencesSortedByCreatedDateAscending_ForValidTenantAndProjectIds()
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

            var response = await client.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?sortOrder=asc");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);

            result.Result.Count().ShouldBe(100);

            for (int i = 0; i < result.Result.Count(); i++)
            {
                if (i != 0)
                {
                    var previousGeofence = result.Result.ElementAt(i - 1);
                    var currentGeofence = result.Result.ElementAt(i);
                    previousGeofence.CreatedDate.ShouldBeLessThan(currentGeofence.CreatedDate);
                }
            }
        }

        [Fact]
        public async Task GetAllGeofences_Returns50PaginatedGeofencesSortedByCreatedDateAscending_ForValidTenantAndProjectIds()
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

            var response = await client.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?sortOrder=asc&pageCount=50");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);

            result.Result.Count().ShouldBe(50);

            for (int i = 0; i < result.Result.Count(); i++)
            {
                if (i != 0)
                {
                    var previousGeofence = result.Result.ElementAt(i - 1);
                    var currentGeofence = result.Result.ElementAt(i);
                    previousGeofence.CreatedDate.ShouldBeLessThan(currentGeofence.CreatedDate);
                }
            }
        }

        [Fact]
        public async Task GetAllGeofences_Returns10PaginatedGeofencesSortedByExternalIdAscending_ForValidTenantAndProjectIds()
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

            var response = await client.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?sortOrder=asc&orderBy=externalid&pageCount=10");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
            result.Result.Count().ShouldBe(10);

            var expectedTopTen = Seed.TenantId1_ProjectId1_Geofences.OrderBy(g => g.ExternalId).Take(10);

            for (int i = 0; i < result.Result.Count(); i++)
            {
                var actual = result.Result.ElementAt(i);
                var expected = expectedTopTen.ElementAt(i);
                actual.ExternalId.ShouldBe(expected.ExternalId);
            }
        }

        [Fact]
        public async Task GetAllGeofences_Returns10PaginatedGeofencesSortedByExternalIdDescending_ForValidTenantAndProjectIds()
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

            var response = await client.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?sortOrder=desc&orderBy=externalid&pageCount=10");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
            result.Result.Count().ShouldBe(10);

            var expectedTopTen = Seed.TenantId1_ProjectId1_Geofences.OrderByDescending(g => g.ExternalId).Take(10);

            for (int i = 0; i < result.Result.Count(); i++)
            {
                var actual = result.Result.ElementAt(i);
                var expected = expectedTopTen.ElementAt(i);
                actual.ExternalId.ShouldBe(expected.ExternalId);
            }
        }

        [Fact]
        public async Task GetAllGeofences_Returns10PaginatedGeofencesSortedByShapeAscending_ForValidTenantAndProjectIds()
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

            var response = await client.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?sortOrder=asc&orderBy=shape&pageCount=10");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
            result.Result.Count().ShouldBe(10);

            var expectedTopTen = Seed.TenantId1_ProjectId1_Geofences.OrderBy(g => g.Shape).OrderByDescending(g => g.CreatedDate).Take(10);

            for (int i = 0; i < result.Result.Count(); i++)
            {
                var actual = result.Result.ElementAt(i);
                var expected = expectedTopTen.ElementAt(i);
                actual.ExternalId.ShouldBe(expected.ExternalId);
            }
        }

        [Fact]
        public async Task GetAllGeofences_Returns10PaginatedGeofencesSortedByShapeDescending_ForValidTenantAndProjectIds()
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

            var response = await client.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?sortOrder=desc&orderBy=shape&pageCount=10");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
            result.Result.Count().ShouldBe(10);

            var expectedTopTen = Seed.TenantId1_ProjectId1_Geofences.OrderByDescending(g => g.Shape).OrderByDescending(g => g.CreatedDate).Take(10);

            for (int i = 0; i < result.Result.Count(); i++)
            {
                var actual = result.Result.ElementAt(i);
                var expected = expectedTopTen.ElementAt(i);
                actual.ExternalId.ShouldBe(expected.ExternalId);
            }
        }

        [Fact]
        public async Task GetAllGeofences_Returns10PaginatedGeofencesFromPage2_ForValidTenantAndProjectIds()
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

            var response = await client.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?page=2&pageCount=10");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
            result.Result.Count().ShouldBe(10);

            var expectedTopTen = Seed.TenantId1_ProjectId1_Geofences.OrderByDescending(g => g.CreatedDate).Skip(20).Take(10);

            for (int i = 0; i < result.Result.Count(); i++)
            {
                var actual = result.Result.ElementAt(i);
                var expected = expectedTopTen.ElementAt(i);
                actual.ExternalId.ShouldBe(expected.ExternalId);
            }
        }

        [Fact]
        public async Task GetAllGeofences_Returns10PaginatedGeofencesFromPage10_ForValidTenantAndProjectIds()
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

            var response = await client.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?page=10&pageCount=10");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
            result.Result.Count().ShouldBe(10);

            var expectedTopTen = Seed.TenantId1_ProjectId1_Geofences.OrderByDescending(g => g.CreatedDate).Skip(100).Take(10);

            for (int i = 0; i < result.Result.Count(); i++)
            {
                var actual = result.Result.ElementAt(i);
                var expected = expectedTopTen.ElementAt(i);
                actual.ExternalId.ShouldBe(expected.ExternalId);
            }
        }

        [Fact]
        public async Task GetAllGeofences_Returns400_WhenInvalidParam()
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

            var response = await client.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?page=-1");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
            result.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
       }

        [Fact]
        public async Task GetAllGeofences_Returns500_ForRepositoryException()
        {
            var mockRepo = new Mock<IGeofenceRepository>();
            mockRepo.Setup(r => r.GetPaginatedGeofencesByProjectId(Seed.TenantId1, Seed.TenantId1_ProjectId1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddTransient<IGeofenceRepository>((_) => mockRepo.Object);
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test", options => {});
                });
            }).CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await client.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
            result.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Services.Geofences.Data;
using Shouldly;
using Xunit;

namespace Ranger.Services.Geofences.Tests.IntegrationTests
{
    public class ControllerTests : IClassFixture<FixtureResolver>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly GeofencesFixture _fixture;

        public ControllerTests(FixtureResolver fixture)
        {
            _factory = fixture.Factory;
            _fixture = fixture.Fixture;
       }

        [Fact]
        public async Task GetAllGeofences_ReturnsSingleGeofences_ForValidExternalId()
        {
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var externalId = Seed.TenantId1_ProjectId1_Geofences[0].ExternalId;
            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?externalId={externalId}");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<GeofenceResponseModel>>(content);

            result.StatusCode.ShouldBe(StatusCodes.Status200OK);
            result.Result.ExternalId.ShouldBe(externalId);
       }

        [Fact]
        public async Task GetAllGeofences_Returns404_ForInvalidExternalId()
        {
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var externalId = Seed.TenantId1_ProjectId1_Geofences[0].ExternalId;
            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?externalId={Guid.NewGuid().ToString()}");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<GeofenceResponseModel>>(content);

            result.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
       }

        [Fact]
        public async Task GetAllGeofences_Returns100PaginatedGeofencesSortedByCreatedDateDescending_ForValidTenantAndProjectIds()
        {
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}");
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
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?sortOrder=asc");
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
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?sortOrder=asc&pageCount=50");
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
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?sortOrder=asc&orderBy=externalid&pageCount=10");
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
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?sortOrder=desc&orderBy=externalid&pageCount=10");
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
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?sortOrder=asc&orderBy=shape&pageCount=10");
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
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?sortOrder=desc&orderBy=shape&pageCount=10");
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
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?page=2&pageCount=10");
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
        public async Task GetAllGeofences_Returns1PaginatedGeofencesFromPage10_ForValidTenantAndProjectIds()
        {
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?page=10&pageCount=10");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
            result.Result.Count().ShouldBe(1);

            var expectedTopTen = Seed.TenantId1_ProjectId1_Geofences.OrderByDescending(g => g.CreatedDate).Skip(100).Take(10);

            for (int i = 0; i < result.Result.Count(); i++)
            {
                var actual = result.Result.ElementAt(i);
                var expected = expectedTopTen.ElementAt(i);
                actual.ExternalId.ShouldBe(expected.ExternalId);
            }
        }


        [Fact]
        public async Task GetAllGeofences_ReturnAllGeofences_InBoundingBox()
        {
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var bounds = new List<LngLat>
                {
                    new LngLat(-81.61998, 41.54433),
                    new LngLat(-81.61724, 41.45489),
                    new LngLat(-81.47300, 41.45386),
                    new LngLat(-81.46888, 41.56693)
                }.Select(f => JsonConvert.SerializeObject(f));
            var queryBounds = String.Join(';', bounds);

            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?bounds={queryBounds}");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
            result.Result.Count().ShouldBe(Seed.TenantId1_ProjectId1_Geofences.Count());
        }

        [Fact]
        public async Task GetAllGeofences_ReturnNoGeofences_WhenBoundingBoxNewYork()
        {
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var bounds = new List<LngLat>
                {
                    new LngLat(-73.98022359345703, 40.71905316959156),
                    new LngLat(-74.03172200654296, 40.71905316959156),
                    new LngLat(-74.03172200654296, 40.70649683843022),
                    new LngLat(-73.98022359345703, 40.70649683843022)
                }.Select(f => JsonConvert.SerializeObject(f));
            var queryBounds = String.Join(';', bounds);

            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?bounds={queryBounds}");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
            result.Result.Count().ShouldBe(0);
        }

        [Fact]
        public async Task GetAllGeofences_ReturnNoGeofences_OutsideBoundingBox()
        {
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var bounds = new List<LngLat>
                {
                    new LngLat(-81.80283, 41.38295),
                    new LngLat(-81.80351, 41.33273),
                    new LngLat(-81.67748, 41.33298),
                    new LngLat(-81.68022, 41.39067)
                }.Select(f => JsonConvert.SerializeObject(f));
            var queryBounds = String.Join(';', bounds);

            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?bounds={queryBounds}");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
            result.Result.Count().ShouldBe(0);
        }

        [Fact]
        public async Task GetAllGeofences_Returns400_WhenInvalidParam()
        {
            _fixture.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _fixture.httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await _fixture.httpClient.GetAsync($"/geofences/{Seed.TenantId1}/{Seed.TenantId1_ProjectId1}?page=-1");
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
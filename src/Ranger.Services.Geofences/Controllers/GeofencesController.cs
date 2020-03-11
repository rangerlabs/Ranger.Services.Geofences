using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Services.Geofences.Data;

namespace Ranger.Services.Geofences.Controllers {
    [ApiController]
    public class GeofencesController : ControllerBase {
        private readonly IGeofenceRepository geofenceRepository;
        private readonly ITenantsClient tenantsClient;
        private readonly ILogger<GeofencesController> logger;

        public GeofencesController (IGeofenceRepository geofenceRepository, ITenantsClient tenantsClient, ILogger<GeofencesController> logger) {
            this.geofenceRepository = geofenceRepository;
            this.tenantsClient = tenantsClient;
            this.logger = logger;
        }

        [HttpGet ("/{domain}/geofences")]
        public async Task<IActionResult> GetAllGeofences ([FromRoute] string domain, [FromQuery] Guid projectId) {
            ContextTenant tenant = null;
            try {
                tenant = await this.tenantsClient.GetTenantAsync<ContextTenant> (domain);
            } catch (HttpClientException ex) {
                if ((int) ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound) {
                    return NotFound ();
                }
            } catch (Exception ex) {
                this.logger.LogError (ex, "An exception occurred retrieving the ContextTenant object. Cannot construct the tenant specific repository.");
                return StatusCode (StatusCodes.Status500InternalServerError);
            }

            try {
                var result = await this.geofenceRepository.GetAllGeofencesByProjectId (tenant.DatabaseUsername, projectId);
                return Ok (result);
            } catch (Exception ex) {
                this.logger.LogError (ex, $"An exception occurred retrieving the requested geofences for domain '{domain}' and projectId '{projectId}'");
                return StatusCode (StatusCodes.Status500InternalServerError);
            }
        }
    }
}
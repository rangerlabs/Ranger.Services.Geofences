using FluentValidation;
using Xunit;

namespace Ranger.Services.Geofences.Tests
{

    [Collection("Validation collection")]
    public class CreateGeofenceValidationTests
    {
        private readonly IValidator<CreateGeofence> geofenceValidator;
        public CreateGeofenceValidationTests(ValidationFixture fixture)
        {
            this.geofenceValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<CreateGeofence>>();
        }
    }
}
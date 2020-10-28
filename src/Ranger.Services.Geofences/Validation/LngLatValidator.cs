using FluentValidation;
using Ranger.Common;

namespace Ranger.Services.Geofences
{
    public class LngLatValidator : AbstractValidator<LngLat>
    {
        public LngLatValidator()
        {
            RuleFor(ll => ll.Lat).NotNull().InclusiveBetween(-90, 90);
            RuleFor(ll => ll.Lng).NotNull().InclusiveBetween(-180, 180);
        }
    }
}
using FluentValidation;
using Ranger.Common;

namespace Ranger.Services.Geofences
{
    public class DailyScheduleValidator : AbstractValidator<DailySchedule>
    {
        public DailyScheduleValidator()
        {
            RuleFor(d => d).Custom((r, c) =>
            {
                var instance = (c.InstanceToValidate as DailySchedule);
                if (instance.StartTime > instance.EndTime)
                {
                    c.AddFailure("StartTime must be before EndTime");
                }
            });
            RuleFor(d => d.StartTime).NotNull();
            RuleFor(d => d.EndTime).NotNull();
        }
    }
}
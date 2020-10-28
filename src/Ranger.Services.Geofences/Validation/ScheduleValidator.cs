using FluentValidation;
using NodaTime;
using Ranger.Common;

namespace Ranger.Services.Geofences
{
    public class ScheduleValidator : AbstractValidator<Schedule>
    {
        public ScheduleValidator(IValidator<DailySchedule> dailyScheduleValidator)
        {
            RuleFor(s => s.TimeZoneId).NotEmpty().Custom((t, c) =>
            {
                if (DateTimeZoneProviders.Tzdb.GetZoneOrNull(t) is null)
                {
                    c.AddFailure("TimezoneId was invalid");
                }
            });
            RuleFor(s => s.Sunday).NotEmpty().SetValidator(dailyScheduleValidator);
            RuleFor(s => s.Monday).NotEmpty().SetValidator(dailyScheduleValidator);
            RuleFor(s => s.Tuesday).NotEmpty().SetValidator(dailyScheduleValidator);
            RuleFor(s => s.Wednesday).NotEmpty().SetValidator(dailyScheduleValidator);
            RuleFor(s => s.Thursday).NotEmpty().SetValidator(dailyScheduleValidator);
            RuleFor(s => s.Friday).NotEmpty().SetValidator(dailyScheduleValidator);
            RuleFor(s => s.Saturday).NotEmpty().SetValidator(dailyScheduleValidator);
        }
    }
}
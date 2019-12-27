using System;

namespace Ranger.Services.Geofences.Data
{
    public class Schedule
    {
        public Tuple<DateTime, DateTime> Monday { get; set; }
        public Tuple<DateTime, DateTime> Tuesday { get; set; }
        public Tuple<DateTime, DateTime> Wednesday { get; set; }
        public Tuple<DateTime, DateTime> Thursday { get; set; }
        public Tuple<DateTime, DateTime> Friday { get; set; }
        public Tuple<DateTime, DateTime> Saturday { get; set; }
        public Tuple<DateTime, DateTime> Sunday { get; set; }

    }
}
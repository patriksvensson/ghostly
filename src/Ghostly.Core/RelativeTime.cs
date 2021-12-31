using System;

namespace Ghostly
{
    public sealed class RelativeTime
    {
        public long Quantity { get; }
        public RelativeTimeUnit Unit { get; }

        private const int Second = 1;
        private const int Minute = 60 * Second;
        private const int Hour = 60 * Minute;
        private const int Day = 24 * Hour;
        private const int Month = 30 * Day;

        public RelativeTime(int quantity, RelativeTimeUnit unit)
        {
            Quantity = quantity;
            Unit = unit;
        }

        public static RelativeTime GetRelativeTime(IClock clock, DateTime timestamp)
        {
            if (clock is null)
            {
                throw new ArgumentNullException(nameof(clock));
            }

            var now = clock.Now().ToUniversalTime().DateTime;
            var span = now - timestamp.EnsureUniversalTime();
            var delta = Math.Abs(span.TotalSeconds);

            if (delta < 1 * Minute)
            {
                return new RelativeTime(span.Seconds, RelativeTimeUnit.Second);
            }

            if (delta < 60 * Minute)
            {
                return new RelativeTime(span.Minutes, RelativeTimeUnit.Minute);
            }

            if (delta < 24 * Hour)
            {
                return new RelativeTime(span.Hours, RelativeTimeUnit.Hour);
            }

            if (delta < 30 * Day)
            {
                return new RelativeTime(span.Days, RelativeTimeUnit.Day);
            }

            if (delta < 12 * Month)
            {
                var months = Convert.ToInt32(Math.Floor((double)span.Days / 30));
                return new RelativeTime(months, RelativeTimeUnit.Month);
            }

            var years = Math.Max(1, Convert.ToInt32(Math.Floor((double)span.Days / 365)));
            return new RelativeTime(years, RelativeTimeUnit.Year);
        }
    }
}

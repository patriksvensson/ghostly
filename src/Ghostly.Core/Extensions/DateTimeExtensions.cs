using System;
using Ghostly.Core.Services;

namespace Ghostly
{
    public static class DateTimeExtensions
    {
        public static DateTime EnsureUniversalTime(this DateTime time)
        {
            if (time.Kind == DateTimeKind.Utc)
            {
                return time;
            }

            if (time.Kind == DateTimeKind.Local)
            {
                return time.ToUniversalTime();
            }

            return DateTime.SpecifyKind(time, DateTimeKind.Utc);
        }

        public static string Humanize(this DateTime timestamp, IClock clock, ILocalizer localizer)
        {
            if (clock is null)
            {
                throw new ArgumentNullException(nameof(clock));
            }

            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            var relative = RelativeTime.GetRelativeTime(clock, timestamp);
            return RelativeTimeHumanizer.Emit(relative, localizer).ToLowerInvariant();
        }
    }
}

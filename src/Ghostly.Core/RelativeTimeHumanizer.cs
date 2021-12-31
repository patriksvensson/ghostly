using System;
using Ghostly.Core.Services;

namespace Ghostly
{
    public static class RelativeTimeHumanizer
    {
        public static string Emit(RelativeTime time, ILocalizer localizer)
        {
            if (time is null)
            {
                throw new ArgumentNullException(nameof(time));
            }

            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            if (time.Quantity == 1 && time.Unit == RelativeTimeUnit.Day)
            {
                return localizer["Time_Yesterday"];
            }

            if (time.Quantity == 1)
            {
                return localizer[$"Time_One{GetUnitName(time.Unit)}Ago"];
            }

            return localizer.Format($"Time_{GetUnitName(time.Unit)}sAgo", time.Quantity);
        }

        private static string GetUnitName(RelativeTimeUnit unit)
        {
            switch (unit)
            {
                case RelativeTimeUnit.Year:
                    return "Year";
                case RelativeTimeUnit.Month:
                    return "Month";
                case RelativeTimeUnit.Day:
                    return "Day";
                case RelativeTimeUnit.Hour:
                    return "Hour";
                case RelativeTimeUnit.Minute:
                    return "Minute";
                case RelativeTimeUnit.Second:
                    return "Second";
            }

            throw new InvalidOperationException();
        }
    }
}

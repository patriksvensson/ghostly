using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace Ghostly.Uwp.Converters
{
    public sealed class TimestampToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime timestamp)
            {
                var now = DateTime.UtcNow;

                // Convert the timestamp to local time.
                timestamp = timestamp.EnsureUniversalTime().ToLocalTime();

                if (timestamp.Date == now.Date)
                {
                    return timestamp.ToString("t", CultureInfo.CurrentCulture);
                }

                if ((now - timestamp).Days < 7)
                {
                    // Day of week and time
                    return string.Concat(timestamp.ToString("ddd", CultureInfo.CurrentCulture), " ", timestamp.ToString("t", CultureInfo.CurrentCulture));
                }
                else
                {
                    // Date
                    return timestamp.ToString("d", CultureInfo.CurrentCulture);
                }
            }

            throw new InvalidOperationException("Not a timestamp!");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}

using System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Ghostly.Uwp.Converters
{
    public sealed class NotificationTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(parameter is string parameterString) || string.IsNullOrWhiteSpace(parameterString))
            {
                throw new InvalidOperationException("No parameter specified.");
            }

            if (!(value is bool unread))
            {
                throw new InvalidOperationException("Not a notification");
            }

            if (parameterString == "Foreground")
            {
                return unread
                    ? (Brush)Application.Current.Resources["Text100Highlight"]
                    : (Brush)Application.Current.Resources["Text100"];
            }
            else if (parameterString == "FontWeight")
            {
                return unread
                    ? FontWeights.SemiBold
                    : FontWeights.Normal;
            }
            else
            {
                throw new InvalidOperationException("Unknown parameter");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
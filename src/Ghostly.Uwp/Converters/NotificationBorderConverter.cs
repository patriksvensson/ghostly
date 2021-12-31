using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Ghostly.Uwp.Converters
{
    public sealed class NotificationBorderConverter : IValueConverter
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

            if (parameterString == "Margin")
            {
                return unread ? new Thickness(1, 0, 0, 0) : new Thickness(1, 0, 0, 0);
            }
            else if (parameterString == "Padding")
            {
                return unread ? new Thickness(10, 0, 0, 0) : new Thickness(13, 0, 0, 0);
            }
            else if (parameterString == "BorderThickness")
            {
                return unread ? new Thickness(3, 0, 0, 0) : new Thickness(0, 0, 0, 0);
            }
            else if (parameterString == "BorderBrush")
            {
                return unread
                    ? (Brush)Application.Current.Resources["NotificationUnreadBorder"]
                    : (Brush)Application.Current.Resources["RegionBrush"];
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
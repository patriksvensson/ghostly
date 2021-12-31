using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Ghostly.Uwp.Converters
{
    public sealed class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool result = false;
            if (value is bool)
            {
                result = (bool)value;
            }
            else if (value is bool?)
            {
                result = (bool?)value ?? false;
            }

            return result ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility)
            {
                return (Visibility)value != Visibility.Visible;
            }
            else
            {
                return false;
            }
        }
    }
}

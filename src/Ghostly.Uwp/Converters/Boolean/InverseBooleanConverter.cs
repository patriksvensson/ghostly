using System;
using Windows.UI.Xaml.Data;

namespace Ghostly.Uwp.Converters
{
    public sealed class InverseBooleanConverter : IValueConverter
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

            return !result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            else
            {
                return false;
            }
        }
    }
}

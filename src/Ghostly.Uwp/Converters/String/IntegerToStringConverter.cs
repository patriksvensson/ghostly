using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace Ghostly.Uwp.Converters
{
    public sealed class IntegerToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int integer)
            {
                return integer.ToString(CultureInfo.InvariantCulture);
            }

            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

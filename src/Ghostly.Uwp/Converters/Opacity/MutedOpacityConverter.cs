using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace Ghostly.Uwp.Converters
{
    public sealed class MutedOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolean)
            {
                var offOpacity = 0.5;
                if (parameter is string parameterString && !string.IsNullOrWhiteSpace(parameterString))
                {
                    offOpacity = double.Parse(parameterString, CultureInfo.InvariantCulture);
                }

                return boolean ? offOpacity : 1.0;
            }

            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

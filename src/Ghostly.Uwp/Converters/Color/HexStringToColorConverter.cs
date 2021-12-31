using System;
using Ghostly.Core;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Ghostly.Uwp.Converters
{
    public sealed class HexStringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue)
            {
                if (!string.IsNullOrWhiteSpace(stringValue))
                {
                    return GetSolidColorBrush(stringValue);
                }
            }

            return null;
        }

        public static SolidColorBrush GetSolidColorBrush(string hex)
        {
            var color = ColorRepresentation.ParseHex(hex);
            return new SolidColorBrush(Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}

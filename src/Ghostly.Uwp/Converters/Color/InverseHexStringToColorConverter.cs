using System;
using Ghostly.Core;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Ghostly.Uwp.Converters
{
    public sealed class InverseHexStringToColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush _white = new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush _black = new SolidColorBrush(Colors.Black);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue)
            {
                if (!string.IsNullOrWhiteSpace(stringValue))
                {
                    var components = GetColorComponents(stringValue);
                    var luminance = (float)((0.2126 * components[0]) + (0.7152 * components[1]) + (0.0722 * components[2]));
                    return luminance < 140 ? _white : _black;
                }
            }

            return null;
        }

        public static byte[] GetColorComponents(string hex)
        {
            var color = ColorRepresentation.ParseHex(hex);
            return new byte[] { color.Red, color.Green, color.Blue };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

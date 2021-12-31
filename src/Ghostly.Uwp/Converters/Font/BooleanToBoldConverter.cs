using System;
using Windows.UI.Text;
using Windows.UI.Xaml.Data;

namespace Ghostly.Uwp.Converters
{
    public sealed class BooleanToBoldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolean)
            {
                return boolean ? GetFontWeight(parameter) : FontWeights.Normal;
            }

            return FontWeights.Normal;
        }

        private FontWeight GetFontWeight(object parameter)
        {
            if (parameter is string stringParameter && !string.IsNullOrWhiteSpace(stringParameter))
            {
                if (stringParameter.Equals("SemiBold", StringComparison.OrdinalIgnoreCase))
                {
                    return FontWeights.SemiBold;
                }
            }

            return FontWeights.Bold;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using Ghostly.Features.Querying;
using Ghostly.Uwp.Strings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Ghostly.Uwp.Converters
{
    public sealed class AutoCompleteItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(parameter is string parameterName) || string.IsNullOrWhiteSpace(parameterName))
            {
                throw new InvalidOperationException("No autocomplete parameter specified.");
            }

            if (value is AutoCompleteItem item)
            {
                if (parameterName.Equals("description", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(item.Description))
                    {
                        return item.Description;
                    }

                    if (!string.IsNullOrWhiteSpace(item.LocalizedDescription))
                    {
                        return Localize.GetString(item.LocalizedDescription);
                    }

                    return null;
                }

                if (parameterName.Equals("visibility", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(item.Description) && string.IsNullOrWhiteSpace(item.LocalizedDescription))
                    {
                        return Visibility.Collapsed;
                    }

                    return Visibility.Visible;
                }
            }

            throw new InvalidOperationException("Invalid autocomplete value.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

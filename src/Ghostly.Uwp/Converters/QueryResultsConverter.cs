using System;
using Ghostly.Uwp.Strings;
using Windows.UI.Xaml.Data;

namespace Ghostly.Uwp.Converters
{
    public sealed class QueryResultsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                var id = "Query_Results_None";
                if (count == 1)
                {
                    id = "Query_Results_Singular";
                }
                else if (count > 1)
                {
                    id = "Query_Results_Plural";
                }

                return Localize.Format(id, count);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

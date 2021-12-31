using System;
using System.Collections;
using Ghostly.Core.Mvvm;
using Ghostly.Domain;
using Ghostly.Uwp.Utilities;
using Windows.UI.Xaml.Data;

namespace Ghostly.Uwp.Converters
{
    public class IncrementalLoadingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IIncrementalLoadingSource<Notification> coreIncremental)
            {
                if (value is IList)
                {
                    return new IncrementalLoadingSourceAdapter<Notification>(coreIncremental);
                }
            }

            throw new InvalidOperationException("Not an incremental loading source!");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}

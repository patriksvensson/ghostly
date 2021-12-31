using System;
using System.Windows.Input;
using Windows.UI.Xaml.Data;

namespace Ghostly.Uwp.Converters
{
    public sealed class CommandToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ICommand boolean)
            {
                return boolean.CanExecute(parameter);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

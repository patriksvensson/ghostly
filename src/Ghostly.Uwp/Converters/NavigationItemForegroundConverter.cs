using System;
using Ghostly.Core.Mvvm;
using Ghostly.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Ghostly.Uwp.Converters
{
    public sealed class NavigationItemForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is NavigationItemKind item)
            {
                if (item == NavigationItemKind.NewCategory)
                {
                    return (Brush)Application.Current.Resources["NavigationCommandBrush"];
                }
            }

            if (value is NavigationItem menu)
            {
                if (menu.Category != null)
                {
                    if (menu.Category.Inbox)
                    {
                        return (Brush)Application.Current.Resources["NavigationInboxBrush"];
                    }

                    if (menu.Category.Archive)
                    {
                        return (Brush)Application.Current.Resources["NavigationArchiveBrush"];
                    }
                }

                if (menu.Kind == NavigationItemKind.NewCategory)
                {
                    return (Brush)Application.Current.Resources["NavigationCommandBrush"];
                }
            }

            return (Brush)Application.Current.Resources["NavigationItemBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
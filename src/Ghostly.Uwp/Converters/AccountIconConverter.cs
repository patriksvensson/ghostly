using System;
using MahApps.Metro.IconPacks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Ghostly.Uwp.Converters
{
    public sealed class AccountIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(parameter is string parameterString) || string.IsNullOrWhiteSpace(parameterString))
            {
                throw new InvalidOperationException("No parameter specified.");
            }

            if (parameterString == "Icon")
            {
                return PackIconOcticonsKind.MarkGithub;
            }
            else if (parameterString == "Foreground")
            {
                return (Brush)Application.Current.Resources["GitHubIssueOpenBrush"];
            }
            else
            {
                throw new InvalidOperationException("Unknown parameter");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

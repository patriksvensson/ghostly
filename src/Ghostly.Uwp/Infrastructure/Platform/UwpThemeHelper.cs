using System;
using System.Threading.Tasks;
using Ghostly.Core.Pal;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpThemeHelper : IThemeHelper
    {
        private readonly ILocalSettings _settings;

        public UwpThemeHelper(ILocalSettings settings)
        {
            _settings = settings;
        }

        public async Task SetRequestedTheme()
        {
            await SetTheme(GetTheme());
        }

        public Theme GetTheme()
        {
            return _settings.GetAppBackgroundRequestedTheme();
        }

        public async Task SetTheme(Theme theme)
        {
            var elementTheme = (ElementTheme)(int)theme;
            foreach (var view in CoreApplication.Views)
            {
                await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (Window.Current.Content is FrameworkElement frameworkElement)
                    {
                        frameworkElement.RequestedTheme = elementTheme;
                    }
                });
            }

            // Save the requested theme.
            _settings.SetAppBackgroundRequestedTheme(theme);
        }
    }
}

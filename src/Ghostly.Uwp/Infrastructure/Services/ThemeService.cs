using System;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Services;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Ghostly.Uwp.Infrastructure
{
    public sealed class ThemeService : IThemeService, IDisposable, IInitializable
    {
        private readonly ILocalSettings _settings;
        private readonly IGhostlyLog _log;
        private ThemeListener _themeListener;

        public GhostlyTheme Current { get; private set; }
        public GhostlyTheme Canonical { get; private set; }

        public ThemeService(ILocalSettings settings, IGhostlyLog log)
        {
            _settings = settings;
            _log = log;
        }

        public void Dispose()
        {
            _themeListener?.Dispose();
        }

        public Task<bool> Initialize(bool background)
        {
            Current = _settings.GetTheme();
            return Task.FromResult(true);
        }

        public async Task InitializeTheme()
        {
            _themeListener = new ThemeListener();
            _themeListener.ThemeChanged += OnThemeChanged;

            await UpdateTheme(Current, force: true);

            TitlebarService.Instance.Initialize();
        }

        public async Task SetTheme(GhostlyTheme theme)
        {
            if (theme == Current)
            {
                return;
            }

            _settings.SetTheme(theme);
            await UpdateTheme(theme);

            Current = theme;
        }

        private void OnThemeChanged(ThemeListener sender)
        {
            if (Current == GhostlyTheme.Default)
            {
                UpdateTheme(Current).FireAndForgetSafeAsync();
            }
        }

        private async Task UpdateTheme(GhostlyTheme theme, bool force = false)
        {
            if (theme == GhostlyTheme.Default)
            {
                switch (_themeListener.CurrentTheme)
                {
                    case ApplicationTheme.Light:
                        theme = GhostlyTheme.Light;
                        break;
                    case ApplicationTheme.Dark:
                        theme = GhostlyTheme.Dark;
                        break;
                }
            }

            // Set the canonical theme.
            Canonical = theme;

            if (!force)
            {
                if (theme == Current)
                {
                    return;
                }
            }

            _log.Debug("Setting theme to {ThemeName}.", theme.GetName());

            // We need to update resources on the UI thread.
            await Platform.ThreadingModel.ExecuteOnUIThread(() =>
            {
                // Remove dictionaries
                Application.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri("ms-appx:///Themes/Dark.xaml") });
                Application.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri("ms-appx:///Themes/Light.xaml") });

                // Add theme dictionary
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri($"ms-appx:///Themes/{theme.GetCanonicalName()}.xaml") });
            });

            // Update all views.
            foreach (var view in CoreApplication.Views)
            {
                await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (Window.Current.Content is FrameworkElement frameworkElement)
                    {
                        frameworkElement.RequestedTheme = theme.ToElementTheme();
                    }
                });
            }

            // Set the theme as the current one.
            Current = theme;
        }
    }
}

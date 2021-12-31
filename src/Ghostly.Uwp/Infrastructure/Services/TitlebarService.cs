using System;
using Ghostly.Core.Mvvm;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Ghostly.Uwp.Infrastructure
{
    public sealed class TitlebarService : Observable
    {
        private readonly CoreApplicationViewTitleBar _titlebar;

        public event EventHandler LayoutChanged = (s, e) => { };

        public static TitlebarService Instance { get; }
        static TitlebarService()
        {
            Instance = new TitlebarService();
        }

        public TitlebarService()
        {
            _titlebar = CoreApplication.GetCurrentView().TitleBar;
            _titlebar.LayoutMetricsChanged += OnLayoutMetricsChanged;
            _visibility = Visibility.Visible;

            RefreshTilebarOffset();
        }

        private Thickness _position;
        public Thickness Position
        {
            get => _position;
            set
            {
                SetProperty(ref _position, value);
                LayoutChanged(this, EventArgs.Empty);
            }
        }

        private Thickness _top;
        public Thickness Top
        {
            get => _top;
            set
            {
                SetProperty(ref _top, value);
            }
        }

        private Visibility _visibility;
        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                SetProperty(ref _visibility, value);
                RefreshTilebarOffset();
            }
        }

        private bool _fullscreen;
        public bool FullScreen
        {
            get => _fullscreen;
            set
            {
                if (value && !_fullscreen)
                {
                    // Enter fullscreen.
                    SetProperty(ref _fullscreen, value);
                    Visibility = Visibility.Collapsed;
                }
                else if (!value && _fullscreen)
                {
                    // Exit fullscreen.
                    SetProperty(ref _fullscreen, value);
                    Visibility = Visibility.Visible;
                }
            }
        }

        public void Initialize()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (titleBar != null)
            {
                titleBar.ForegroundColor = Colors.LightGray;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonForegroundColor = Colors.LightGray;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveForegroundColor = Colors.LightGray;

                // Extend into title bar.
                _titlebar.ExtendViewIntoTitleBar = true;
            }
        }

        private void OnLayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            RefreshTilebarOffset();
        }

        private void RefreshTilebarOffset()
        {
            var leftPosition = _titlebar.SystemOverlayLeftInset;
            var height = _titlebar.Height;

            // top position should be 6 pixels for a 32 pixel high titlebar hence scale by actual height
            var correctHeight = height / 32 * 6;

            if (_fullscreen)
            {
                Top = new Thickness(0, correctHeight, 0, 0);
            }
            else
            {
                Top = new Thickness(0, _titlebar.Height, 0, 0);
            }

            Position = new Thickness(leftPosition + 12, correctHeight, 0, 0);
        }
    }
}

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core.Mvvm;
using Ghostly.Domain;
using Ghostly.Uwp.Infrastructure;
using Ghostly.Uwp.Strings;
using Ghostly.Uwp.Utilities;
using Ghostly.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace Ghostly.Uwp.Views
{
    public sealed partial class MainView : Page, INotifyPropertyChanged, IInitializableView
    {
        private readonly KeyboardAccelerator _focusAccelerator;
        private readonly KeyboardAccelerator _markAsReadAccelerator;
        private Notification _menuNotification; // Workaround for the Share API

        public TitlebarService Titlebar => TitlebarService.Instance;
        public StreamUriResolver UriResolver { get; }

        public ListViewSelectionMode SelectionMode { get; set; }
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainViewModel), typeof(MainView), null);

        public MainView()
        {
            InitializeComponent();

            MasterDetailsView.MultipleSelectionChanged += OnMultipleItemsSelected;
            MasterDetailsView.MapDetails += item => ViewModel.GetNotificationDetails(item as Notification).GetAwaiter().GetResult();

            _focusAccelerator = new KeyboardAccelerator { Modifiers = VirtualKeyModifiers.Control, Key = VirtualKey.F };
            _focusAccelerator.Invoked += (_, __) => QueryBox?.Focus(FocusState.Programmatic);

            _markAsReadAccelerator = new KeyboardAccelerator { Modifiers = VirtualKeyModifiers.Control, Key = VirtualKey.Q };
            _markAsReadAccelerator.Invoked += (_, __) => ViewModel?.MarkSelectedAsRead();

            SelectionMode = ListViewSelectionMode.Extended;
            UriResolver = new StreamUriResolver();
        }

        public Task InitializeView(object context)
        {
            if (context is MainViewModel vm)
            {
                ViewModel = vm;
                vm.QueryCleared += OnQueryCleared;
                vm.SynchronizationStateChanged += OnSynchronizationStateChanged;
            }

            // This is the worst API ever...
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;

            return Task.CompletedTask;
        }

        private void OnSynchronizationStateChanged(object sender, Domain.Messages.SynchronizationStateChanged e)
        {
            if (e.IsSynchronizing)
            {
                if (e.IsSynchronizingSingle)
                {
                    IconRotation2.Begin();
                }
                else
                {
                    IconRotation.Begin();
                }
            }
            else
            {
                if (e.IsSynchronizingSingle)
                {
                    IconRotation2.Stop();
                }
                else
                {
                    IconRotation.Stop();
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (KeyboardAccelerators.Count == 0)
            {
                KeyboardAccelerators.Add(_markAsReadAccelerator);
                KeyboardAccelerators.Add(_focusAccelerator);
                ToolTipService.SetToolTip(_focusAccelerator, string.Empty);
            }
        }

        private void OnMultipleItemsSelected(object sender, System.Collections.Generic.IList<object> e)
        {
            var notifications = e?.Cast<Notification>()?.ToArray() ?? Array.Empty<Notification>();
            ViewModel.SelectedItems = notifications;
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                var panel = (Grid)((Grid)sender).FindName("HoverPanelGrid");
                if (panel.Visibility == Visibility.Collapsed)
                {
                    panel.Visibility = Visibility.Visible;
                }

                var storyboard = panel.Resources["EnterStoryboard"] as Storyboard;
                storyboard.Begin();
            }
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                var storyboard = ((Grid)((Grid)sender).FindName("HoverPanelGrid")).Resources["ExitStoryboard"] as Storyboard;
                storyboard.Begin();
            }
        }

        private void Grid_Loading(FrameworkElement sender, object args)
        {
            var storyboard = ((Grid)((Grid)sender).FindName("HoverPanelGrid")).Resources["ExitStoryboard"] as Storyboard;
            storyboard.Begin();
        }

        private void Menu_Opening(object sender, object args)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as Notification;
            MenuArchive.IsEnabled = ViewModel.ArchiveCommand.CanExecute(item);
            MenuMarkAsRead.IsEnabled = ViewModel.MarkAsReadCommand.CanExecute(item);
            MenuStar.IsEnabled = ViewModel.StarCommand.CanExecute(item);
            MenuStar.Visibility = MenuStar.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
            MenuUnstar.IsEnabled = ViewModel.UnstarCommand.CanExecute(item);
            MenuUnstar.Visibility = MenuUnstar.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
            MenuMute.IsEnabled = ViewModel.MuteCommand.CanExecute(item);
            MenuMute.Visibility = MenuMute.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
            MenuUnmute.IsEnabled = ViewModel.UnmuteCommand.CanExecute(item);
            MenuUnmute.Visibility = MenuUnmute.IsEnabled ? Visibility.Visible : Visibility.Collapsed;

            var hideSeparator = !MenuStar.IsEnabled && !MenuUnstar.IsEnabled && !MenuMute.IsEnabled && !MenuUnmute.IsEnabled;
            MenuSeparator.Visibility = hideSeparator ? Visibility.Collapsed : Visibility.Visible;
        }

        private void MenuOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as Notification;
            ViewModel.OpenInBrowserCommand.Execute(item);
        }

        private void MenuArchive_Click(object sender, RoutedEventArgs e)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as Notification;
            ViewModel.ArchiveCommand.Execute(item);
        }

        private void MenuMarkAsRead_Click(object sender, RoutedEventArgs e)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as Notification;
            ViewModel.MarkAsReadCommand.Execute(item);
        }

        private void MenuMove_Click(object sender, RoutedEventArgs e)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as Notification;
            ViewModel.MoveCommand.Execute(item);
        }

        private void MenuStar_Click(object sender, RoutedEventArgs e)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as Notification;
            ViewModel.StarCommand.Execute(item);
        }

        private void MenuUnstar_Click(object sender, RoutedEventArgs e)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as Notification;
            ViewModel.UnstarCommand.Execute(item);
        }

        private void MenuMute_Click(object sender, RoutedEventArgs e)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as Notification;
            ViewModel.MuteCommand.Execute(item);
        }

        private void MenuUnmute_Click(object sender, RoutedEventArgs e)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as Notification;
            ViewModel.UnmuteCommand.Execute(item);
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri != null && args.Uri.Scheme != "ms-local-stream")
            {
                args.Cancel = true;
                ViewModel.OnLinkClicked(args.Uri).FireAndForgetSafeAsync();
            }
        }

        private void ButtonSyncNotification_Click(object sender, RoutedEventArgs e)
        {
            var selected = MasterDetailsView.SelectedItem as Notification;
            ViewModel.SyncSingleCommand.Execute(selected);
        }

        private void ButtonOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            var selected = MasterDetailsView.SelectedItem as Notification;
            ViewModel.OpenInBrowserCommand.Execute(selected);
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Event")]
        private void CheckList_Checked(object sender, RoutedEventArgs e)
        {
            SelectionMode = ListViewSelectionMode.Multiple;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectionMode)));
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Event")]
        private void CheckList_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectionMode = ListViewSelectionMode.Extended;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectionMode)));
        }

        private void MasterDetailsView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var selected = MasterDetailsView.SelectedItem as Notification;
            ViewModel.OpenInBrowserCommand.Execute(selected);
        }

        private void OnQueryCleared(object sender, EventArgs e)
        {
            QueryControl.ClearQuery();
        }

        private void QueryControl_OnQueryCleared(object sender, Controls.QueryClearedEventArgs e)
        {
            ViewModel.OnQuery(null).FireAndForgetSafeAsync();
        }

        private void QueryControl_OnQuerySubmitted(object sender, Controls.QuerySubmittedEventArgs e)
        {
            ViewModel.OnQuery(e.QueryText).FireAndForgetSafeAsync();
        }

        private void SwipeMove_Invoked(Microsoft.UI.Xaml.Controls.SwipeItem sender, Microsoft.UI.Xaml.Controls.SwipeItemInvokedEventArgs args)
        {
            if (args?.SwipeControl?.DataContext is Notification notification)
            {
                if (ViewModel.MoveCommand.CanExecute(notification))
                {
                    ViewModel.MoveCommand.Execute(notification);
                }
            }
        }

        private void SwipeOpenInBrowser_Invoked(Microsoft.UI.Xaml.Controls.SwipeItem sender, Microsoft.UI.Xaml.Controls.SwipeItemInvokedEventArgs args)
        {
            if (args?.SwipeControl?.DataContext is Notification notification)
            {
                if (ViewModel.OpenInBrowserCommand.CanExecute(notification))
                {
                    ViewModel.OpenInBrowserCommand.Execute(notification);
                }
            }
        }

        private void MenuShare_Click(object sender, RoutedEventArgs e)
        {
            if ((Menu?.Target as ListViewItem)?.Content is Notification notification)
            {
                _menuNotification = notification;
                ShowShareDialog();
            }
        }

        private void ButtonShare_Click(object sender, RoutedEventArgs e)
        {
            _menuNotification = null;
            ShowShareDialog();
        }

        private void ShowShareDialog()
        {
            if (!DataTransferManager.IsSupported())
            {
                _menuNotification = null;
                return;
            }

            var theme = ShareUITheme.Default;

            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                if (frameworkElement.RequestedTheme == ElementTheme.Dark)
                {
                    theme = ShareUITheme.Dark;
                }

                if (frameworkElement.RequestedTheme == ElementTheme.Light)
                {
                    theme = ShareUITheme.Light;
                }
            }

            DataTransferManager.ShowShareUI(new ShareUIOptions
            {
                Theme = theme,
            });
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var notification = _menuNotification ?? ViewModel.SelectedItem;
            if (notification?.Url != null)
            {
                var request = args.Request;
                request.Data.Properties.Title = Localize.GetString("Main_ShareGitHubLink");
                request.Data.Properties.Description = notification.Title ?? string.Empty;
                request.Data.SetWebLink(notification.Url);
            }

            _menuNotification = null;
        }
    }
}

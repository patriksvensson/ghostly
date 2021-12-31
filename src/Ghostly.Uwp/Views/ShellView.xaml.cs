using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core.Mvvm;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Uwp.Infrastructure;
using Ghostly.Uwp.Strings;
using Ghostly.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinUi = Microsoft.UI.Xaml.Controls;

namespace Ghostly.Uwp.Views
{
    public sealed partial class ShellView : Page, IShellView, IView, IInitializableView, INotifyPropertyChanged
    {
        public UIElement ShellElement => this;
        public Frame ShellFrame => NavigationFrame;
        public ShellViewModel ViewModel => DataContext as ShellViewModel;

        public event TypedEventHandler<ShellView, Rect> TogglePaneButtonRectChanged;
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        public TitlebarService Titlebar => TitlebarService.Instance;
        public NavigationViewModel Navigation => ViewModel.Navigation;

        public Rect TogglePaneButtonRect { get; private set; }

        public Stateful<bool> ShowUnreadIndicators { get; set; }

        private int _navigationOffset;
        public int NavigationOffset
        {
            get => _navigationOffset;
            set
            {
                _navigationOffset = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(NavigationOffset)));
            }
        }

        private int _toggleOffset;
        public int ToggleOffset
        {
            get => _toggleOffset;
            set
            {
                _toggleOffset = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ToggleOffset)));
            }
        }

        public ShellView()
        {
            InitializeComponent();

            ShowUnreadIndicators = new Stateful<bool>(false);

            LayoutUpdated += OnLayoutUpdated;
            Titlebar.LayoutChanged += OnTitlebarLayoutChanged;
        }

        public Task InitializeView(object context)
        {
            InAppNotification.StackMode = StackMode.QueueBehind;

            RootSplitView.PaneOpened += OnPaneOpened;
            RootSplitView.PaneOpening += OnPaneOpened;
            RootSplitView.PaneClosed += OnPaneClosed;
            RootSplitView.PaneClosing += OnPaneClosed;

            UpdateUnreadIndicators(RootSplitView.IsPaneOpen);

            return Task.CompletedTask;
        }

        private void OnPaneClosed(SplitView sender, object args)
        {
            UpdateUnreadIndicators(false);
        }

        private void OnPaneOpened(SplitView sender, object args)
        {
            UpdateUnreadIndicators(true);
        }

        private void UpdateUnreadIndicators(bool isOpen)
        {
            if (isOpen)
            {
                if (RootSplitView.DisplayMode != SplitViewDisplayMode.CompactInline)
                {
                    ShowUnreadIndicators.Value = true;
                }
                else
                {
                    ShowUnreadIndicators.Value = false;
                }
            }
            else
            {
                ShowUnreadIndicators.Value = true;
            }
        }

        public void UpdateNavigation(NavigationEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (e.Parameter is NavigationItemInvokedEventArgs parameter)
            {
                // Category?
                if (parameter.Kind == NavigationItemKind.Category)
                {
                    var index = Navigation.Categories.IndexOf(Navigation.Categories.FirstOrDefault(c => c.Id == parameter.Id.Value));
                    if (index != -1)
                    {
                        MenuListView.SelectedIndex = -1;
                        CategoryListView.SelectedIndex = index;
                    }
                }

                // Inbox?
                if (parameter.Kind == NavigationItemKind.Inbox)
                {
                    var index = Navigation.Categories.IndexOf(Navigation.Categories.FirstOrDefault(c => c?.Category.Inbox ?? false));
                    if (index != -1)
                    {
                        MenuListView.SelectedIndex = -1;
                        CategoryListView.SelectedIndex = index;
                    }
                }
                else
                {
                    // Must be menu command.
                    var index = Navigation.Menu.IndexOf(Navigation.Menu.FirstOrDefault(x => x.Kind == parameter.Kind));
                    if (index != -1)
                    {
                        MenuListView.SelectedIndex = index;
                        CategoryListView.SelectedIndex = -1;
                    }
                }
            }
        }

        public UIElement GetHandle()
        {
            return this;
        }

        public Frame GetFrame()
        {
            return NavigationFrame;
        }

        public WinUi.NavigationView GetNavigationView()
        {
            throw new NotSupportedException();
        }

        public void ShowInAppNotification(string message, int timeout)
        {
            InAppNotification.Show(message, timeout);
        }

        private void OnLayoutUpdated(object sender, object e)
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                Titlebar.FullScreen = true;
            }
            else
            {
                Titlebar.FullScreen = false;
            }
        }

        private void OnTitlebarLayoutChanged(object sender, EventArgs e)
        {
            ToggleOffset = (int)Titlebar.Top.Top;
            NavigationOffset = ToggleOffset + 48;
        }

        private void CheckTogglePaneButtonSizeChanged()
        {
            TogglePaneButtonRect =
                RootSplitView.DisplayMode == SplitViewDisplayMode.Inline ||
                RootSplitView.DisplayMode == SplitViewDisplayMode.Overlay
                    ? TogglePaneButton.TransformToVisual(this).TransformBounds(
                        new Rect(0, 0, TogglePaneButton.ActualWidth, TogglePaneButton.ActualHeight))
                    : default;

            TogglePaneButtonRectChanged?.Invoke(this, TogglePaneButtonRect);
        }

        private void Menu_Opening(object sender, object e)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as NavigationItem;
            if (item != null)
            {
                MenuEdit.Text = item.Category.Kind == CategoryKind.Filter ? Localize.GetString("Shell_EditFilter") : Localize.GetString("Shell_EditCategory");
                MenuEdit.IsEnabled = item.IsDeletable;

                MenuMarkAllAsRead.IsEnabled = item.Count > 0;
                MenuMarkAllAsRead.Visibility = Visibility.Visible;

                MenuArchiveAll.IsEnabled = item.Category.Inbox;
                MenuArchiveAll.Visibility = item.Category.Inbox ? Visibility.Visible : Visibility.Collapsed;

                MenuDelete.Text = item.Category.Kind == CategoryKind.Filter ? Localize.GetString("Shell_DeleteFilter") : Localize.GetString("Shell_DeleteCategory");
                MenuDelete.IsEnabled = item.IsDeletable;
            }
        }

        private void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as NavigationItem;
            if (item != null)
            {
                ViewModel.DeleteCategoryCommand.Execute(item.Category);
            }
        }

        private void MenuArchiveAll_Click(object sender, RoutedEventArgs e)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as NavigationItem;
            if (item != null)
            {
                ViewModel.ArchiveCategoryCommand.Execute(item.Category);
            }
        }

        private void MenuMarkAllAsRead_Click(object sender, RoutedEventArgs e)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as NavigationItem;
            if (item != null)
            {
                ViewModel.MarkCategoryAsReadCommand.Execute(item.Category);
            }
        }

        private void MenuEdit_Click(object sender, RoutedEventArgs e)
        {
            var item = (Menu?.Target as ListViewItem)?.Content as NavigationItem;
            if (item != null)
            {
                ViewModel.EditCategoryCommand.Execute(item.Category);
            }
        }

        private void CategoryListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            var index = 0;
            var ordering = new List<CategoryOrder>();
            foreach (var item in Navigation.Categories)
            {
                ordering.Add(new CategoryOrder(item.Id.Value, index));
                index += 10;
            }

            ViewModel.ReorderCategories(ordering).FireAndForgetSafeAsync();
        }
    }
}
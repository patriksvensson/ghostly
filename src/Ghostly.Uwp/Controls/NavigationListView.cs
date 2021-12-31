using System;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Ghostly.Uwp.Controls
{
    public class NavigationListView : ListView
    {
        private SplitView _splitViewHost;

        public event EventHandler<ListViewItem> ItemInvoked;

        public bool ShowTopBorder { get; set; }

        public NavigationListView()
        {
            SelectionMode = ListViewSelectionMode.Single;
            IsItemClickEnabled = true;
            ItemClick += ItemClickedHandler;

            // Locate the hosting SplitView control
            Loaded += (s, a) =>
            {
                var parent = VisualTreeHelper.GetParent(this);
                while (parent != null && !(parent is Page) && !(parent is SplitView))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent != null && parent is SplitView)
                {
                    _splitViewHost = parent as SplitView;

                    _splitViewHost.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, (sender, args) =>
                    {
                        OnPaneToggled();
                    });

                    // Call once to ensure we're in the correct state
                    OnPaneToggled();
                }
            };
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Remove the entrance animation on the item containers.
            for (int i = 0; i < ItemContainerTransitions.Count; i++)
            {
                if (ItemContainerTransitions[i] is EntranceThemeTransition)
                {
                    ItemContainerTransitions.RemoveAt(i);
                }
            }
        }

        public void SetSelectedItem(ListViewItem item)
        {
            int index = -1;
            if (item != null)
            {
                index = IndexFromContainer(item);
            }

            for (int i = 0; i < Items.Count; i++)
            {
                var lvi = (ListViewItem)ContainerFromIndex(i);
                if (lvi != null)
                {
                    if (i != index)
                    {
                        lvi.IsSelected = false;
                    }
                    else if (i == index)
                    {
                        lvi.IsSelected = true;
                    }
                }
            }
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            var focusedItem = FocusManager.GetFocusedElement();

            switch (e.Key)
            {
                case VirtualKey.Up:
                    TryMoveFocus(FocusNavigationDirection.Up);
                    e.Handled = true;
                    break;

                case VirtualKey.Down:
                    TryMoveFocus(FocusNavigationDirection.Down);
                    e.Handled = true;
                    break;

                case VirtualKey.Tab:
                    var shiftKeyState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);
                    var shiftKeyDown = (shiftKeyState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                    // If we're on the header item then this will be null and we'll still get the default behavior.
                    if (focusedItem is ListViewItem currentItem)
                    {
                        bool onlastitem = currentItem != null && IndexFromContainer(currentItem) == Items.Count - 1;
                        bool onfirstitem = currentItem != null && IndexFromContainer(currentItem) == 0;

                        if (!shiftKeyDown)
                        {
                            if (onlastitem)
                            {
                                TryMoveFocus(FocusNavigationDirection.Next);
                            }
                            else
                            {
                                TryMoveFocus(FocusNavigationDirection.Down);
                            }
                        }

                        // Shift + Tab
                        else
                        {
                            if (onfirstitem)
                            {
                                TryMoveFocus(FocusNavigationDirection.Previous);
                            }
                            else
                            {
                                TryMoveFocus(FocusNavigationDirection.Up);
                            }
                        }
                    }
                    else if (focusedItem is Control)
                    {
                        if (!shiftKeyDown)
                        {
                            TryMoveFocus(FocusNavigationDirection.Down);
                        }

                        // Shift + Tab
                        else
                        {
                            TryMoveFocus(FocusNavigationDirection.Up);
                        }
                    }

                    e.Handled = true;
                    break;

                case VirtualKey.Space:
                case VirtualKey.Enter:
                    // Fire our event using the item with current keyboard focus
                    InvokeItem(focusedItem);
                    e.Handled = true;
                    break;

                default:
                    base.OnKeyDown(e);
                    break;
            }
        }

        private void TryMoveFocus(FocusNavigationDirection direction)
        {
            if (direction == FocusNavigationDirection.Next || direction == FocusNavigationDirection.Previous)
            {
                FocusManager.TryMoveFocus(direction);
            }
            else
            {
                var control = FocusManager.FindNextFocusableElement(direction) as Control;
                if (control != null)
                {
                    control.Focus(FocusState.Programmatic);
                }

                if (control is ListViewItem)
                {
                    var item = control as ListViewItem;
                    ScrollIntoView(item.Content);
                }
            }
        }

        private void ItemClickedHandler(object sender, ItemClickEventArgs e)
        {
            // Triggered when the item is selected using something other than a keyboard
            var item = ContainerFromItem(e.ClickedItem);
            InvokeItem(item);
        }

        private void InvokeItem(object focusedItem)
        {
            SetSelectedItem(focusedItem as ListViewItem);
            ItemInvoked?.Invoke(this, focusedItem as ListViewItem);

            if (_splitViewHost == null || _splitViewHost.IsPaneOpen)
            {
                if (_splitViewHost != null &&
                    (_splitViewHost.DisplayMode == SplitViewDisplayMode.CompactOverlay ||
                    _splitViewHost.DisplayMode == SplitViewDisplayMode.Overlay))
                {
                    _splitViewHost.IsPaneOpen = false;
                }

                if (focusedItem is ListViewItem)
                {
                    ((ListViewItem)focusedItem).Focus(FocusState.Programmatic);
                }
            }
        }

        private void OnPaneToggled()
        {
            if (ItemsPanelRoot == null)
            {
                return;
            }

            if (_splitViewHost.IsPaneOpen)
            {
                if (ShowTopBorder)
                {
                    BorderThickness = new Thickness(0, 1, 0, 0);
                }

                ItemsPanelRoot.ClearValue(FrameworkElement.WidthProperty);
                ItemsPanelRoot.ClearValue(FrameworkElement.HorizontalAlignmentProperty);
            }
            else if (_splitViewHost.DisplayMode == SplitViewDisplayMode.CompactInline ||
                _splitViewHost.DisplayMode == SplitViewDisplayMode.CompactOverlay)
            {
                if (ShowTopBorder)
                {
                    BorderThickness = new Thickness(0, 0, 0, 0);
                }

                ItemsPanelRoot.SetValue(FrameworkElement.WidthProperty, _splitViewHost.CompactPaneLength);
                ItemsPanelRoot.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            }
        }
    }
}

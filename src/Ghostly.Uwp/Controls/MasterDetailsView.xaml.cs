using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Ghostly.Core;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Ghostly.Uwp.Controls
{
    public partial class MasterDetailsView : Microsoft.Toolkit.Uwp.UI.Controls.ListDetailsView
    {
        public static readonly DependencyProperty GroupTemplateProperty =
            DependencyProperty.Register(nameof(GroupTemplate), typeof(DataTemplate),
                typeof(MasterDetailsView), new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(IList<object>),
                typeof(MasterDetailsView), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register(nameof(SelectionMode), typeof(ListViewSelectionMode),
                typeof(MasterDetailsView), new PropertyMetadata(ListViewSelectionMode.Extended));

        public static readonly DependencyProperty ListContextFlyoutProperty =
            DependencyProperty.Register(nameof(ListContextFlyout), typeof(FlyoutBase),
                typeof(MasterDetailsView), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedBackgroundProperty =
            DependencyProperty.Register(nameof(SelectedBackground), typeof(Color),
                typeof(MasterDetailsView), new PropertyMetadata(null));

        public event EventHandler<IList<object>> MultipleSelectionChanged = (s, e) => { };

        public DataTemplate GroupTemplate
        {
            get { return (DataTemplate)GetValue(GroupTemplateProperty); }
            set { SetValue(GroupTemplateProperty, value); }
        }

        public IList<object> SelectedItems
        {
            get { return (IList<object>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public ListViewSelectionMode SelectionMode
        {
            get { return (ListViewSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        public FlyoutBase ListContextFlyout
        {
            get { return (FlyoutBase)GetValue(ListContextFlyoutProperty); }
            set { SetValue(ListContextFlyoutProperty, value); }
        }

        public Color SelectedBackground
        {
            get { return (Color)GetValue(SelectedBackgroundProperty); }
            set { SetValue(SelectedBackgroundProperty, value); }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var listview = GetTemplateChild("MasterList") as ListView;
            listview.SelectionChanged += (s, e) =>
            {
                Platform.ThreadingModel.ExecuteOnUIThread(() =>
                {
                    MultipleSelectionChanged?.Invoke(listview, listview.SelectedItems);
                });

                string state;
                string noSelectionState;
                string hasSelecitonState;
                if (ActualWidth < CompactModeThresholdWidth)
                {
                    state = "NarrowState";
                    noSelectionState = "NoSelectionNarrow";
                    hasSelecitonState = "HasSelectionNarrow";
                }
                else
                {
                    state = "WideState";
                    noSelectionState = "NoSelectionWide";
                    hasSelecitonState = "HasSelectionWide";
                }

                var count = listview?.SelectedItems.Count;
                if (count != 1)
                {
                    VisualStateManager.GoToState(this, noSelectionState, true);
                    VisualStateManager.GoToState(this, state, true);
                }
                else
                {
                    VisualStateManager.GoToState(this, hasSelecitonState, true);
                    VisualStateManager.GoToState(this, state, true);
                }
            };

            // Apply the group template
            if (GroupTemplate != null)
            {
                if (listview.GroupStyle.Count == 0)
                {
                    listview.GroupStyle.Add(new GroupStyle()
                    {
                        HeaderTemplate = GroupTemplate,
                    });
                }
            }
        }
    }
}

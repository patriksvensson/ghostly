using Ghostly.Uwp.Infrastructure;
using Ghostly.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Ghostly.Uwp.Views
{
    public sealed partial class RuleView : Page
    {
        public TitlebarService Titlebar => TitlebarService.Instance;
        public RuleViewModel ViewModel => (RuleViewModel)DataContext;

        public RuleView()
        {
            InitializeComponent();
        }

        private void ListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            ViewModel.UpdateSortOrder()
                .FireAndForgetSafeAsync();
        }
    }
}

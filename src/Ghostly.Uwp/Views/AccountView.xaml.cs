using Ghostly.Uwp.Infrastructure;
using Windows.UI.Xaml.Controls;

namespace Ghostly.Uwp.Views
{
    public sealed partial class AccountView : Page
    {
        public TitlebarService Titlebar
        {
            get
            {
                return TitlebarService.Instance;
            }
        }

        public AccountView()
        {
            InitializeComponent();
        }
    }
}

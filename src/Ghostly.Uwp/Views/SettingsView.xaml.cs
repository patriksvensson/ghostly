using Ghostly.Uwp.Infrastructure;
using Ghostly.Uwp.Utilities;
using Windows.UI.Xaml.Controls;

namespace Ghostly.Uwp.Views
{
    public sealed partial class SettingsView : Page
    {
        public TitlebarService Titlebar => TitlebarService.Instance;
        public string Architecture => CompilationInfo.Architecture;

        public SettingsView()
        {
            InitializeComponent();
        }
    }
}

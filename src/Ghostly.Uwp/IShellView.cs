using Ghostly.Core.Mvvm;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Ghostly.Uwp
{
    public interface IShellView : IShell, IView
    {
        UIElement GetHandle();
        Frame GetFrame();
        void UpdateNavigation(NavigationEventArgs e);
    }
}

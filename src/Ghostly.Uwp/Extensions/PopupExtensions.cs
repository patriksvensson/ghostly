using Windows.UI.Core;
using Windows.UI.Xaml.Controls.Primitives;

namespace Ghostly.Uwp
{
    internal static class PopupExtensions
    {
        public static void Open(this Popup popup)
        {
            if (popup != null)
            {
                popup.IsOpen = true;
            }
        }

        public static void Close(this Popup popup)
        {
            if (popup != null)
            {
                popup.IsOpen = false;
            }
        }

        public static void Close(this Popup popup, CoreDispatcher dispatcher)
        {
            dispatcher.FireAndForgetSafe(() =>
            {
                if (popup != null)
                {
                    popup.IsOpen = false;
                }
            });
        }
    }
}

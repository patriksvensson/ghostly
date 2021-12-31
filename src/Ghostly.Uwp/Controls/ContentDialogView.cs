using Ghostly.Core.Mvvm;
using Windows.UI.Xaml.Controls;

namespace Ghostly.Uwp.Controls
{
    public abstract class ContentDialogView : ContentDialog
    {
        protected ContentDialogView()
        {
            Opened += OnOpened;
            Closing += OnClosing;
        }

        private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (DataContext is IDialogViewModel dialog)
            {
                dialog.OnShown().FireAndForgetSafeAsync();
            }
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary)
            {
                if (DataContext != null && DataContext is IValidatableDialog validatable)
                {
                    if (!validatable.Validate())
                    {
                        args.Cancel = true;
                    }
                }
            }
        }
    }
}

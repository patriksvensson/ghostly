using Ghostly.Core.Pal;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpToastNotifier : IToastNotifier
    {
        public void Show(string title, string content)
        {
            var visual = new ToastVisual
            {
                BindingGeneric = new ToastBindingGeneric
                {
                    Children =
                    {
                        new AdaptiveText
                        {
                            Text = title,
                            HintStyle = AdaptiveTextStyle.Title,
                        },
                        new AdaptiveText
                        {
                            Text = content,
                            HintStyle = AdaptiveTextStyle.Body,
                        },
                    },
                },
            };

            var toastContent = new ToastContent()
            {
                Visual = visual,
            };

            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}

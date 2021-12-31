using System;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Mvvm;
using Ghostly.ViewModels;
using Windows.ApplicationModel.Activation;

namespace Ghostly.Uwp.Activation
{
    public sealed class ToastActivationHandler : ActivationHandler<ToastNotificationActivatedEventArgs>
    {
        private readonly INavigationService _navigator;
        private readonly IGhostlyLog _log;

        public ToastActivationHandler(INavigationService navigator, IGhostlyLog log)
        {
            _navigator = navigator;
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public override async Task Handle(ToastNotificationActivatedEventArgs args)
        {
            _log.Debug("[ToastActivationHandler] BackgroundActivated={BackgroundActivated}", GhostlyState.IsBackgroundActivated);
            await _navigator.Navigate<MainViewModel>(new NavigationItemInvokedEventArgs(NavigationItemKind.Inbox));
        }
    }
}

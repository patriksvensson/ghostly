using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Mvvm;
using Ghostly.Core.Pal;
using Ghostly.Data.Models;
using Ghostly.ViewModels;
using MediatR;
using Windows.ApplicationModel.Activation;

namespace Ghostly.Uwp.Activation
{
    public sealed class LaunchActivationHandler : MultipleActivationHandler
    {
        private readonly IMediator _mediator;
        private readonly IMarketHelper _market;
        private readonly INavigationService _navigator;
        private readonly IGhostlyLog _log;

        public LaunchActivationHandler(
            IMediator mediator,
            IMarketHelper market,
            INavigationService navigator,
            IGhostlyLog log)
                : base(
                      typeof(LaunchActivatedEventArgs),
                      typeof(StartupTaskActivatedEventArgs))
        {
            _mediator = mediator;
            _market = market;
            _navigator = navigator;
            _log = log;
        }

        protected override bool CanHandle(object args)
        {
            return !_navigator.HasContent;
        }

        protected override async Task Handle(object args)
        {
            _log.Debug("[LaunchActivationHandler] BackgroundActivated={BackgroundActivated}", GhostlyState.IsBackgroundActivated);

            // TODO: Clean up navigation args.
            if (_market.IsFirstRun() || !(await _mediator.GetAccounts()).Any(a => a.State != AccountState.Deleted))
            {
                // Show account view.
                await _navigator.Navigate<AccountViewModel>(
                    new NavigationItemInvokedEventArgs(NavigationItemKind.Account, programatically: true));
            }
            else
            {
                // Show first category.
                await _navigator.Navigate<MainViewModel>(
                    new NavigationItemInvokedEventArgs(NavigationItemKind.Inbox, programatically: true));
            }
        }
    }
}

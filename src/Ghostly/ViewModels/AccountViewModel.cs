using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Mvvm;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Services;
using Ghostly.ViewModels.Commands;
using MediatR;

namespace Ghostly.ViewModels
{
    public sealed class AccountViewModel : Screen
    {
        private readonly IAuthenticationService _authenticator;
        private readonly IMediator _mediator;
        private readonly ISynchronizationService _synchronizer;
        private readonly INavigationService _navigator;
        private readonly ITelemetry _telemetry;

        public ObservableCollection<Account> Accounts { get; }
        public AsyncRelayCommand LoginToGitHubCommand { get; }
        public AsyncRelayCommand LoginToPublicGitHubCommand { get; }
        public AsyncRelayCommand<Account> SignOutCommand { get; }
        public AsyncRelayCommand<Account> OpenInBrowserCommand { get; }
        public ICommand ImportSettingsCommand { get; }
        public ICommand ExportSettingsCommand { get; }

        public bool Disconnected { get; private set; }

        public AccountViewModel(
            IAuthenticationService authenticator,
            IMediator mediator,
            INetworkHelper network,
            ISynchronizationService synchronizer,
            IMessageService messenger,
            INavigationService navigation,
            IDialogService dialogs,
            IProfileService profiles,
            ITelemetry telemetry,
            ILocalizer localizer,
            IEnumerable<IVendor> vendors)
        {
            if (network is null)
            {
                throw new ArgumentNullException(nameof(network));
            }

            if (messenger is null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }

            _authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _synchronizer = synchronizer ?? throw new ArgumentNullException(nameof(synchronizer));
            _navigator = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));

            Accounts = new ObservableCollection<Account>();
            Disconnected = !network.IsConnected;

            // Commands
            LoginToGitHubCommand = new AsyncRelayCommand(() => LoginToGitHub(Vendor.GitHub), () => !Disconnected);
            SignOutCommand = new AsyncRelayCommand<Account>(SignOut, _ => true);
            OpenInBrowserCommand = new AsyncRelayCommand<Account>(OpenInBrowser, _ => true);
            ImportSettingsCommand = new ImportProfileCommand(dialogs, profiles, messenger, localizer, vendors);
            ExportSettingsCommand = new ExportProfileCommand(dialogs, profiles, messenger, network, localizer, vendors);

            // Subscribe for account updates.
            messenger.SubscribeOnUIThreadAsync<AccountUpdated>(OnAccountUpdated);
            messenger.SubscribeOnUIThread<NetworkConnectivityChanged>(OnNetworkConnectivityChanged);
            messenger.SubscribeOnUIThread<AccountStateChanged>(OnAccountStateChanged);
        }

        protected override async Task OnInitialize()
        {
            var accounts = await _mediator.GetAccounts();
            foreach (var account in accounts.Where(a => a.State != AccountState.Deleted))
            {
                Accounts.Add(account);
            }
        }

        private async Task LoginToGitHub(Vendor vendor)
        {
            await _authenticator.Login(vendor);
        }

        private async Task SignOut(Account account)
        {
            var accounts = await _mediator.GetAccounts();
            var model = accounts.SingleOrDefault(a => a.Id == account.Id);
            if (model != null)
            {
                // Track event.
                _telemetry.TrackEvent(Constants.TrackingEvents.SignOut, new Dictionary<string, string>
                {
                    { "Vendor", model.VendorKind.ToString() },
                    { "State", model.State.ToString() },
                });

                // Sign out from account.
                await _authenticator.Logout(model);

                // Mark account as deleted.
                model.State = AccountState.Deleted;
                await _mediator.UpdateAccount(model);

                // Remove item from collection.
                Accounts.Remove(account);
            }

            NotifyPropertyChanged(nameof(Accounts));
        }

        private async Task OpenInBrowser(Account account)
        {
            var accounts = await _mediator.GetAccounts();
            var model = accounts.SingleOrDefault(a => a.Id == account.Id);
            if (model != null)
            {
                await _authenticator.OpenInBrowser(model);
            }
        }

        private async Task OnAccountUpdated(AccountUpdated message)
        {
            if (!Accounts.Any(x => x.Id == message.Account.Id) && message.Account.State != AccountState.Deleted)
            {
                Accounts.Add(message.Account);
            }
            else
            {
                // TODO: What's happening here...
                var account = Accounts.FirstOrDefault(x => x.Id == message.Account.Id);
                if (account != null)
                {
                    account = message.Account;
                }
            }

            // Newly added?
            if (message.Account?.LastSyncedAt == null)
            {
                // Trigger synchronization.
                _synchronizer.Trigger().FireAndForgetSafeAsync();

                // Navigate to the inbox.
                await _navigator.Navigate<MainViewModel>(
                    new NavigationItemInvokedEventArgs(
                        NavigationItemKind.Inbox, programatically: true));
            }
        }

        private void OnAccountStateChanged(AccountStateChanged message)
        {
            var account = Accounts.SingleOrDefault(x => x.Id == message.AccountId);
            if (account != null)
            {
                account.State = message.State;
            }
        }

        private void OnNetworkConnectivityChanged(NetworkConnectivityChanged arg)
        {
            Disconnected = !arg.IsConnected;

            NotifyPropertyChanged(nameof(Disconnected));
            NotifyPropertyChanged(nameof(LoginToGitHubCommand));
            NotifyPropertyChanged(nameof(LoginToPublicGitHubCommand));
        }
    }
}

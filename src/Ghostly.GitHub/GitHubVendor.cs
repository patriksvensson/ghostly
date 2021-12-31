using System;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Data;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;
using Ghostly.Features.Synchronization;
using Ghostly.GitHub.Octokit;

namespace Ghostly.GitHub
{
    internal sealed class GitHubVendor : Vendor<GitHubAccount>
    {
        private readonly IGitHubGatewayFactory _factory;
        private readonly GitHubAccountProvider _accountProvider;
        private readonly GitHubSynchronizer _synchronizer;
        private readonly GitHubProfileService _profiles;
        private readonly IPasswordVault _vault;
        private readonly IGhostlyLog _log;

        public override IVendorProfiles Profiles => _profiles;

        public GitHubVendor(
            IGitHubGatewayFactory factory,
            GitHubAccountProvider accountProvider,
            GitHubSynchronizer synchronizer,
            GitHubProfileService profiles,
            IPasswordVault vault,
            IGhostlyLog log)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _accountProvider = accountProvider ?? throw new ArgumentNullException(nameof(accountProvider));
            _synchronizer = synchronizer ?? throw new ArgumentNullException(nameof(synchronizer));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _vault = vault ?? throw new ArgumentNullException(nameof(vault));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public override bool Matches(Vendor vendor)
        {
            return vendor == Vendor.GitHub;
        }

        public override Task OpenInBrowser(Account model)
        {
            return _accountProvider.OpenInBrowser(model);
        }

        public override Task<bool> CanLogin()
        {
            return _accountProvider.CanLogin();
        }

        public override Task Login(Vendor vendor)
        {
            return _accountProvider.Login();
        }

        public override Task Logout(Account model)
        {
            return _accountProvider.Logout(model as GitHubAccount);
        }

        public override async Task<bool> MarkAsRead(Notification notification)
        {
            if (!(notification is GitHubNotification githubNotification))
            {
                return false;
            }

            using (var context = new GhostlyContext())
            {
                var account = context.Accounts.Find(notification.AccountId);
                if (account == null)
                {
                    _log.Error("Could not find GitHub account with local ID {GitHubAccountId}.", notification.AccountId);
                    return false;
                }

                // Get the GitHub client.
                var accessToken = _vault.GetGitHubAccessToken(account.Username);
                if (accessToken == null)
                {
                    _log.Error("Could not get GitHub access token from Password vault. Does it exist?");
                    return false;
                }

                var gateway = _factory.Create(accessToken);
                if (gateway.IsRateLimited)
                {
                    _log.Warning("Cannot mark GitHub item as read since we're rate limited.");
                    return false;
                }

                // Mark the notification as read.
                _log.Information("Marking GitHub notification #{GitHubNotificationId} as read.", githubNotification.GitHubId);
                var result = await gateway.MarkAsRead(githubNotification.GitHubId);
                if (result.Faulted)
                {
                    _log.Error("Could not mark GitHub notification #{GitHubNotificationId} as read.", githubNotification.GitHubId);
                    return false;
                }
            }

            return true;
        }

        public override bool CanSynchronize(GitHubAccount model)
        {
            return _synchronizer.CanSynchronize(model);
        }

        public override async Task<SynchronizationStatus> Synchronize(GitHubAccount model)
        {
            return await _synchronizer.Synchronize(model);
        }

        public override async Task<SynchronizationStatus> Synchronize(GitHubAccount model, Notification notification)
        {
            return await _synchronizer.SynchronizeSingle(model, notification);
        }
    }
}

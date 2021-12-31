using System;
using System.Globalization;
using System.Threading.Tasks;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;
using Ghostly.Domain.Messages;
using Ghostly.Features.Synchronization;
using Ghostly.GitHub.Octokit;
using Ghostly.GitHub.Synchronizers;

namespace Ghostly.GitHub
{
    internal sealed class GitHubSynchronizer
    {
        private readonly IGitHubGatewayFactory _github;
        private readonly NotificationSynchronizer _notificationSyncer;
        private readonly IPasswordVault _passwordVault;
        private readonly IMessageService _messenger;
        private readonly INetworkHelper _network;
        private readonly ILocalizer _localizer;

        public GitHubSynchronizer(
            IGitHubGatewayFactory github,
            NotificationSynchronizer notificationSyncer,
            IPasswordVault passwordVault,
            IMessageService messenger,
            INetworkHelper network,
            ILocalizer localizer)
        {
            _github = github;
            _notificationSyncer = notificationSyncer;
            _passwordVault = passwordVault;
            _messenger = messenger;
            _network = network;
            _localizer = localizer;
        }

        public bool CanSynchronize(GitHubAccount account)
        {
            // Don't allow updating too often
            var lastSync = account.LastSyncedAt ?? DateTime.MinValue;
            return (DateTime.UtcNow - lastSync).TotalSeconds >= 3;
        }

        public async Task<SynchronizationStatus> Synchronize(GitHubAccount account)
        {
            if (!CanSynchronize(account))
            {
                return SynchronizationStatus.BackOff;
            }

            var token = _passwordVault.GetGitHubAccessToken(account.Username);
            if (token == null)
            {
                return SynchronizationStatus.RequiresAuthentication;
            }

            // No internet connection?
            if (!_network.IsConnected)
            {
                return SynchronizationStatus.NoInternetConnection;
            }

            // Rate limited?
            var gateway = _github.Create(token);
            if (gateway.IsRateLimited)
            {
                return SynchronizationStatus.RateLimited;
            }

            // Not authorized?
            if (!await gateway.IsAuthorized())
            {
                return SynchronizationStatus.AuthenticationFailed;
            }

            // Get notifications.
            var result = await _notificationSyncer.Synchronize(gateway, account);
            if (result != SynchronizationStatus.Completed)
            {
                if (result == SynchronizationStatus.RateLimited)
                {
                    await ShowRateLimitMessage(gateway.Reset);
                }

                return result;
            }

            // Synchronization completed.
            return SynchronizationStatus.Completed;
        }

        public async Task<SynchronizationStatus> SynchronizeSingle(GitHubAccount account, Notification notification)
        {
            if (!CanSynchronize(account))
            {
                return SynchronizationStatus.BackOff;
            }

            if (!(notification is GitHubNotification githubNotification))
            {
                throw new InvalidOperationException("Not a GitHub notification.");
            }

            var token = _passwordVault.GetGitHubAccessToken(account.Username);
            if (token == null)
            {
                return SynchronizationStatus.RequiresAuthentication;
            }

            // No internet connection?
            if (!_network.IsConnected)
            {
                return SynchronizationStatus.NoInternetConnection;
            }

            // Rate limited?
            var gateway = _github.Create(token);
            if (gateway.IsRateLimited)
            {
                return SynchronizationStatus.RateLimited;
            }

            // Not authorized?
            if (!await gateway.IsAuthorized())
            {
                return SynchronizationStatus.AuthenticationFailed;
            }

            // Get notifications.
            var result = await _notificationSyncer.Synchronize(gateway, account, githubNotification);
            if (result != SynchronizationStatus.Completed)
            {
                if (result == SynchronizationStatus.RateLimited)
                {
                    await ShowRateLimitMessage(gateway.Reset);
                }

                return result;
            }

            // Synchronization completed.
            return SynchronizationStatus.Completed;
        }

        private async Task ShowRateLimitMessage(DateTime reset)
        {
            var time = reset.EnsureUniversalTime().ToLocalTime().ToString("t", CultureInfo.CurrentCulture);
            await _messenger.PublishAsync(new InAppNotification(_localizer.Format("GitHub_RateLimitExceeded", time)));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.GitHub.ViewModels;
using Credentials = Octokit.Credentials;
using GitHubClient = Octokit.GitHubClient;
using OauthLoginRequest = Octokit.OauthLoginRequest;
using OauthToken = Octokit.OauthToken;
using OauthTokenRequest = Octokit.OauthTokenRequest;
using OctokitUser = Octokit.User;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace Ghostly.GitHub
{
    internal sealed class GitHubAuthenticator : IGitHubAuthenticator
    {
        private readonly IMessageService _messenger;
        private readonly IUriLauncher _launcher;
        private readonly IDialogService _dialogs;
        private readonly ITelemetry _telemetry;
        private readonly IGhostlyLog _log;

        public GitHubAuthenticator(
            IMessageService messenger,
            IUriLauncher launcher,
            IDialogService dialogs,
            ITelemetry telemetry,
            IGhostlyLog log)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _log = log;
        }

        public Task OpenInBrowser(Account account)
        {
            var clientId = GitHubSecrets.Instance.ClientId;
            return _launcher.Launch(new Uri($"https://github.com/settings/connections/applications/{clientId}"));
        }

        public async Task Login()
        {
            var preferences = await _dialogs.ShowDialog(new GitHubPreferencesViewModel.Request());
            if (preferences != null)
            {
                await LaunchExternalBrowser(preferences);
            }
        }

        private async Task LaunchExternalBrowser(GitHubAuthenticationPreferences preferences)
        {
            // Track event.
            _telemetry.TrackEvent(Constants.TrackingEvents.SignInBrowser, new Dictionary<string, string>
            {
                { "Vendor", "GitHub" },
            });

            await _launcher.Launch(GetAuthenticateUrl(preferences));
        }

        private static Uri GetAuthenticateUrl(GitHubAuthenticationPreferences preferences)
        {
            var clientId = GitHubSecrets.Instance.ClientId;

            var client = new GitHubClient(new ProductHeaderValue("Ghostly"));
            var request = new OauthLoginRequest(clientId);
            request.Scopes.Add("notifications");
            if (preferences.PrivateRepositories)
            {
                request.Scopes.Add("repo");
            }

            if (preferences.Gists)
            {
                request.Scopes.Add("gist");
            }

            return client.Oauth.GetGitHubLoginUrl(request);
        }

        public async Task Authenticate(string code)
        {
            OctokitUser user;
            var token = default(OauthToken);

            try
            {
                var clientId = GitHubSecrets.Instance.ClientId;
                var clientSecret = GitHubSecrets.Instance.ClientSecret;

                // Get access token from GitHub.
                var client = new GitHubClient(new ProductHeaderValue("Ghostly"));
                token = await client.Oauth.CreateAccessToken(new OauthTokenRequest(clientId, clientSecret, code));

                // Now, validate the token by requesting user information.
                client.Credentials = new Credentials(token.AccessToken);
                user = await client.User.Current();
            }
            catch (Exception ex)
            {
                // Track error.
                _telemetry.TrackAndLogError(_log, "Authentication failed.");
                _telemetry.TrackException(ex, new Dictionary<string, string>
                {
                    { "AccessTokenCreated", (token != null).ToYesNo() },
                });
                return;
            }

            // Notify subscribers that an account have been authenticated.
            await _messenger.PublishAsync(new GitHubUserAuthenticated
            {
                Username = user.Login,
                AvatarUrl = user.AvatarUrl,
                AccessToken = token.AccessToken,
                Scopes = token.Scope.ToArray(),
            });

            // Track event.
            _telemetry.TrackEvent(Constants.TrackingEvents.Authenticated, new Dictionary<string, string>
            {
                { "Scopes", string.Join(", ", token.Scope) },
            });
        }
    }
}

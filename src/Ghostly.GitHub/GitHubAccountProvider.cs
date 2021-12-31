using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Data.Mapping;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;
using MediatR;

namespace Ghostly.GitHub
{
    internal sealed class GitHubAccountProvider
    {
        private readonly IGitHubAuthenticator _authenticator;
        private readonly IMediator _mediator;
        private readonly IPasswordVault _passwordVault;
        private readonly IMessageService _messenger;
        private readonly ILocalizer _localizer;

        public GitHubAccountProvider(
            IGitHubAuthenticator authenticator,
            IMediator mediator,
            IPasswordVault passwordVault,
            IMessageService messenger,
            ILocalizer localizer)
        {
            _authenticator = authenticator;
            _mediator = mediator;
            _passwordVault = passwordVault;
            _messenger = messenger;
            _localizer = localizer;
            _messenger.SubscribeAsync<GitHubUserAuthenticated>(OnAuthenticated);
        }

        public async Task OpenInBrowser(Account account)
        {
            await _authenticator.OpenInBrowser(account);
        }

        public async Task<bool> CanLogin()
        {
            var accounts = await _mediator.GetAccounts();
            return !accounts.OfType<GitHubAccount>().Any();
        }

        public Task Login()
        {
            return _authenticator.Login();
        }

        public Task Logout(GitHubAccount account)
        {
            _passwordVault.DeleteGitHubAccessToken(account.Username);
            return Task.CompletedTask;
        }

        private async Task OnAuthenticated(GitHubUserAuthenticated message)
        {
            // Save access token for user.
            _passwordVault.SaveGitHubAccessToken(message.Username, message.AccessToken);

            // Create account for user
            var accounts = await _mediator.GetAccounts();
            var account = accounts.OfType<GitHubAccount>()
                .SingleOrDefault(x => x.Username == message.Username);

            if (account == null)
            {
                // Create a new account.
                account = new GitHubAccount
                {
                    Username = message.Username,
                    AvatarUrl = message.AvatarUrl,
                    Scopes = message.Scopes,
                    State = AccountState.Active,
                };
            }
            else
            {
                if (account.State == AccountState.Deleted)
                {
                    // Enable the account.
                    account.State = AccountState.Active;
                }

                // Update the scopes.
                account.Scopes = message.Scopes;
            }

            // Update the description.
            var description = AccountMapper.GetDescription(_localizer, account);
            account.SetDescription(description);

            // Update the account.
            await _mediator.UpdateAccount(account);
        }
    }
}

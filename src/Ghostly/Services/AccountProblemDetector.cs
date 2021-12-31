using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Domain.Messages;
using Ghostly.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Services
{
    [DependentOn(typeof(ShellViewModel))]
    [DependentOn(typeof(DatabaseInitializer))]
    public sealed class AccountProblemDetector : IInitializable
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMessageService _messenger;
        private bool _hasAccountProblem;

        public AccountProblemDetector(
            IGhostlyContextFactory factory,
            IMessageService messenger)
        {
            _factory = factory;
            _messenger = messenger;

            messenger.SubscribeAsync<AccountStateChanged>(message => Update());
            messenger.SubscribeAsync<AccountRemoved>(message => Update());
            messenger.SubscribeAsync<AccountUpdated>(message => Update());
        }

        public async Task<bool> Initialize(bool background)
        {
            if (!background)
            {
                await Update();
                return true;
            }

            return false;
        }

        private async Task Update()
        {
            // Keep track of whether or not we had a problem.
            var hadAccountProblem = _hasAccountProblem;

            // Update state.
            using (var context = _factory.Create())
            {
                _hasAccountProblem = await context.Accounts.AnyAsync(a => a.State == AccountState.Unauthorized);
            }

            // Need to notify about new state?
            if (!hadAccountProblem && _hasAccountProblem)
            {
                await _messenger.PublishAsync(new AccountProblemDetected());
            }
            else if (hadAccountProblem && !_hasAccountProblem)
            {
                await _messenger.PublishAsync(new AccountProblemResolved());
            }
        }
    }
}

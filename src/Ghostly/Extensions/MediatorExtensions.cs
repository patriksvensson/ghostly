using System.Collections.Generic;
using System.Threading.Tasks;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Features.Accounts;
using Ghostly.Features.Rules;
using Ghostly.Features.Synchronization;
using MediatR;

namespace Ghostly
{
    public static class MediatorExtensions
    {
        public static async Task<Account> GetAccount(this IMediator mediator, int id)
        {
            if (mediator is null)
            {
                throw new System.ArgumentNullException(nameof(mediator));
            }

            return await mediator.Send(new GetAccountHandler.Request(id));
        }

        public static async Task<IEnumerable<Account>> GetAccounts(this IMediator mediator)
        {
            if (mediator is null)
            {
                throw new System.ArgumentNullException(nameof(mediator));
            }

            return await mediator.Send(new GetAccountsHandler.Request());
        }

        public static async Task UpdateAccount(this IMediator mediator, Account account)
        {
            if (mediator is null)
            {
                throw new System.ArgumentNullException(nameof(mediator));
            }

            await mediator.Send(new UpdateAccountHandler.Request(account));
        }

        public static async Task PerformUpSync(this IMediator mediator)
        {
            if (mediator is null)
            {
                throw new System.ArgumentNullException(nameof(mediator));
            }

            await mediator.Send(new UpSyncHandler.Request());
        }

        public static async Task PerformDownSync(this IMediator mediator, IReadOnlyList<Account> accounts)
        {
            if (mediator is null)
            {
                throw new System.ArgumentNullException(nameof(mediator));
            }

            await mediator.Send(new DownSyncHandler.Request(accounts));
        }

        public static async Task PerformDownSync(this IMediator mediator, Account account, Notification notification)
        {
            if (mediator is null)
            {
                throw new System.ArgumentNullException(nameof(mediator));
            }

            await mediator.Send(new DownSyncHandler.Request(new[] { account }, notification));
        }

        public static async Task UpdateAccountState(this IMediator mediator, int accountId, AccountState state)
        {
            if (mediator is null)
            {
                throw new System.ArgumentNullException(nameof(mediator));
            }

            await mediator.Send(new UpdateAccountStateHandler.Request(accountId, state));
        }

        public static async Task<IReadOnlyList<Rule>> GetRules(this IMediator mediator)
        {
            if (mediator is null)
            {
                throw new System.ArgumentNullException(nameof(mediator));
            }

            return await mediator.Send(new GetRulesHandler.Request());
        }
    }
}

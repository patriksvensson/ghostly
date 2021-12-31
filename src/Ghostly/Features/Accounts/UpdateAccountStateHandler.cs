using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Domain.Messages;
using MediatR;

namespace Ghostly.Features.Accounts
{
    public sealed class UpdateAccountStateHandler : GhostlyRequestHandler<UpdateAccountStateHandler.Request>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMessageService _messenger;

        public UpdateAccountStateHandler(
            IGhostlyContextFactory factory,
            IMessageService messenger)
        {
            _factory = factory;
            _messenger = messenger;
        }

        public sealed class Request : IRequest
        {
            public int AccountId { get; }
            public AccountState State { get; }

            public Request(int accountId, AccountState state)
            {
                AccountId = accountId;
                State = state;
            }
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                var account = await context.Accounts.FindAsync(new object[] { request.AccountId }, cancellationToken);
                if (account == null)
                {
                    return;
                }

                // Update the state.
                account.State = request.State;
                context.Update(account);
                await context.SaveChangesAsync(cancellationToken);

                // Notify subscribers.
                await _messenger.PublishAsync(new AccountStateChanged(request.AccountId, request.State));
            }
        }
    }
}

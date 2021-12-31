using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using MediatR;

namespace Ghostly.Features.Accounts
{
    public sealed class UpdateAccountHandler : GhostlyRequestHandler<UpdateAccountHandler.Request>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMessageService _messenger;

        public UpdateAccountHandler(
            IGhostlyContextFactory factory,
            IMessageService messenger)
        {
            _factory = factory;
            _messenger = messenger;
        }

        public sealed class Request : IRequest
        {
            public Account Account { get; }

            public Request(Account account)
            {
                Account = account;
            }
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                var account = AccountMapper.Map(request.Account);
                if (account.Id == 0)
                {
                    context.Accounts.Add(account);
                }
                else
                {
                    context.Accounts.Update(account);
                }

                // Save the changes to the account.
                await context.SaveChangesAsync(cancellationToken);

                // Update the ID of the model.
                account.Id = account.Id;
                request.Account.Id = account.Id;
            }

            await _messenger.PublishAsync(new AccountUpdated(request.Account));
        }
    }
}

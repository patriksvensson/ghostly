using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Domain;
using MediatR;

namespace Ghostly.Features.Accounts
{
    public sealed class GetAccountHandler : GhostlyRequestHandler<GetAccountHandler.Request, Account>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly ILocalizer _localizer;

        public GetAccountHandler(
            IGhostlyContextFactory factory,
            ILocalizer localizer)
        {
            _factory = factory;
            _localizer = localizer;
        }

        public sealed class Request : IRequest<Account>
        {
            public int AccountId { get; }

            public Request(int accountId)
            {
                AccountId = accountId;
            }
        }

        public override async Task<Account> Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                var account = await context.Accounts.FindAsync(new object[] { request.AccountId }, cancellationToken);
                if (account != null)
                {
                    return AccountMapper.Map(account, _localizer);
                }

                return null;
            }
        }
    }
}

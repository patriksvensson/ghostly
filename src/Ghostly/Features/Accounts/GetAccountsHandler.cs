using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Features.Accounts
{
    public sealed class GetAccountsHandler : GhostlyRequestHandler<GetAccountsHandler.Request, IEnumerable<Account>>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly ILocalizer _localizer;

        public GetAccountsHandler(
            IGhostlyContextFactory factory,
            ILocalizer localizer)
        {
            _factory = factory;
            _localizer = localizer;
        }

        public sealed class Request : IRequest<IEnumerable<Account>>
        {
        }

        public override async Task<IEnumerable<Account>> Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                var accounts = await context.Accounts.ToArrayAsync(cancellationToken);
                return AccountMapper.Map(accounts, _localizer);
            }
        }
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Features.Rules
{
    public sealed class GetRulesHandler : GhostlyRequestHandler<GetRulesHandler.Request, IReadOnlyList<Rule>>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly ILocalizer _localizer;

        public GetRulesHandler(
            IGhostlyContextFactory factory,
            ILocalizer localizer)
        {
            _factory = factory;
            _localizer = localizer;
        }

        public sealed class Request : IRequest<IReadOnlyList<Rule>>
        {
        }

        public override Task<IReadOnlyList<Rule>> Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                var i = context.Rules.Include(r => r.Category);
                var rules = RuleMapper.Map(i, _localizer);
                return Task.FromResult(rules.ToReadOnlyList());
            }
        }
    }
}

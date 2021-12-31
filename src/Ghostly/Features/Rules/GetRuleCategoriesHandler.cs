using System;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class GetRulesForCategory : GhostlyRequestHandler<GetRulesForCategory.Request, IReadOnlyList<Rule>>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly ILocalizer _localizer;

        public sealed class Request : IRequest<IReadOnlyList<Rule>>
        {
            public Category Category { get; }

            public Request(Category category)
            {
                Category = category ?? throw new ArgumentNullException(nameof(category));
            }
        }

        public GetRulesForCategory(
            IGhostlyContextFactory factory,
            ILocalizer localizer)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        public override async Task<IReadOnlyList<Rule>> Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                return RuleMapper.Map(await context.Rules.Include(r => r.Category)
                    .Where(r => r.Category.Id == request.Category.Id)
                    .ToListAsync(cancellationToken), _localizer);
            }
        }
    }
}

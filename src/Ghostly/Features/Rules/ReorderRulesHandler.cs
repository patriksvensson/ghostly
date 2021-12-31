using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Data;
using Ghostly.Domain;
using MediatR;

namespace Ghostly.Features.Rules
{
    public sealed class ReorderRulesHandler : GhostlyRequestHandler<ReorderRulesHandler.Request>
    {
        private readonly IGhostlyContextFactory _factory;

        public sealed class Request : IRequest
        {
            public IReadOnlyList<RuleOrder> Ordering { get; }

            public Request(IEnumerable<RuleOrder> ordering)
            {
                Ordering = ordering.ToReadOnlyList();
            }
        }

        public ReorderRulesHandler(IGhostlyContextFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                var didUpdateSortOrder = false;
                foreach (var rule in context.Rules)
                {
                    var order = request.Ordering.SingleOrDefault(c => c.Id == rule.Id);
                    if (order != null && order.SortOrder != rule.SortOrder)
                    {
                        rule.SortOrder = order.SortOrder;
                        context.Rules.Update(rule);
                        didUpdateSortOrder = true;
                    }
                }

                if (didUpdateSortOrder)
                {
                    await context.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}

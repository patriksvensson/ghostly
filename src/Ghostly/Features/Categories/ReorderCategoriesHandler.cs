using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Data;
using Ghostly.Domain;
using MediatR;

namespace Ghostly.Features.Categories
{
    public sealed class ReorderCategoriesHandler : GhostlyRequestHandler<ReorderCategoriesHandler.Request>
    {
        private readonly IGhostlyContextFactory _factory;

        public sealed class Request : IRequest
        {
            public IReadOnlyList<CategoryOrder> Ordering { get; }

            public Request(IEnumerable<CategoryOrder> ordering)
            {
                Ordering = ordering?.ToReadOnlyList();
            }
        }

        public ReorderCategoriesHandler(
            IGhostlyContextFactory factory)
        {
            _factory = factory;
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                var didUpdateSortOrder = false;
                foreach (var category in context.Categories)
                {
                    var order = request.Ordering.SingleOrDefault(c => c.Id == category.Id);
                    if (order != null && order.SortOrder != category.SortOrder)
                    {
                        category.SortOrder = order.SortOrder;
                        context.Categories.Update(category);
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

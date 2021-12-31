using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Domain;
using MediatR;

namespace Ghostly.Features.Categories
{
    public sealed class GetCategoriesHandler : GhostlyRequestHandler<GetCategoriesHandler.Request, IReadOnlyList<Category>>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly ILocalizer _localizer;

        public GetCategoriesHandler(
            IGhostlyContextFactory factory,
            ILocalizer localizer)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        public sealed class Request : IRequest<IReadOnlyList<Category>>
        {
        }

        public override Task<IReadOnlyList<Category>> Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                // TODO: Optimize
                var categories = context.Categories.ToReadOnlyList();
                return Task.FromResult(CategoryMapper.Map(categories, _localizer).ToReadOnlyList());
            }
        }
    }
}

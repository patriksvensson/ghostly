using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Data;
using Ghostly.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Features.Notifications
{
    public sealed class CheckVisibilityHandler : GhostlyRequestHandler<CheckVisibilityHandler.Request, int[]>
    {
        private readonly IGhostlyContextFactory _factory;

        public CheckVisibilityHandler(IGhostlyContextFactory factory)
        {
            _factory = factory;
        }

        public sealed class Request : IRequest<int[]>
        {
            public int[] Notifications { get; }
            public Category Category { get; }

            public Request(IEnumerable<int> notifications, Category category)
            {
                Notifications = notifications?.ToArray() ?? Array.Empty<int>();
                Category = category;
            }
        }

        public override async Task<int[]> Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                if (request.Category == null || request.Notifications.Length == 0)
                {
                    return request.Notifications;
                }

                if (request.Category.Kind == CategoryKind.Filter)
                {
                    // Return the ID of all notifications visible in the filter.
                    return await context.GetNotificationQuery()
                        .Where(x => request.Notifications.Contains(x.Id))
                        .OptionalWhere(request.Category.Filter)
                        .Select(x => x.Id)
                        .ToArrayAsync(cancellationToken);
                }

                // Return the ID of all notifications visible in the category.
                return await context.GetNotificationQuery()
                    .Where(x => request.Notifications.Contains(x.Id))
                    .Where(x => x.Category.Id == request.Category.Id)
                    .Select(x => x.Id)
                    .ToArrayAsync(cancellationToken);
            }
        }
    }
}

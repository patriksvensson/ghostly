using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Data;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Features.Notifications
{
    public sealed class MoveNotificationsToInboxHandler : GhostlyRequestHandler<MoveNotificationsToInboxHandler.Request>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IGhostlyLog _log;

        public sealed class Request : GhostlyContextRequest
        {
            public int CategoryId { get; }
            public bool SaveChanges { get; set; }

            public Request(int categoryId)
            {
                CategoryId = categoryId;
                SaveChanges = true;
            }
        }

        public MoveNotificationsToInboxHandler(
            IGhostlyContextFactory factory,
            IGhostlyLog log)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            var categoryId = request.CategoryId;

            _log.Information("Moving notifications in category {CategoryId} to inbox.", categoryId);

            using (var context = request.GetOrCreateContext(_factory))
            {
                // Get the inbox category.
                var inbox = await context.Categories.SingleOrDefaultAsync(c => c.Inbox, cancellationToken);
                if (inbox == null)
                {
                    _log.Error("Could not find the inbox category.");
                    return;
                }

                // Move all notifications in this category to the inbox.
                var notifications = context.Notifications.Include(x => x.Category)
                    .Where(x => x.Category.Id == categoryId);

                foreach (var notification in notifications)
                {
                    notification.Category = inbox;
                }

                context.Notifications.UpdateRange(notifications);

                if (request.SaveChanges)
                {
                    await context.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}

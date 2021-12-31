using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Features.Notifications
{
    public sealed class MoveNotificationHandler : GhostlyRequestHandler<MoveNotificationHandler.Request>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMessageService _messenger;
        private readonly ILocalizer _localization;
        private readonly IGhostlyLog _log;

        public MoveNotificationHandler(
            IGhostlyContextFactory factory,
            IMessageService messenger,
            ILocalizer localization,
            IGhostlyLog log)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public sealed class Request : IRequest
        {
            public IReadOnlyList<Notification> Notifications { get; }
            public Category Category { get; }

            public Request(IEnumerable<Notification> notifications, Category category)
            {
                Notifications = notifications as IReadOnlyList<Notification> ?? new List<Notification>(notifications);
                Category = category ?? throw new ArgumentNullException(nameof(category));
            }
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            using (var progress = new IndeterminateProgressReporter(_messenger))
            {
                using (var context = _factory.Create())
                {
                    var category = await GetCategory(request, context);
                    if (category == null)
                    {
                        return;
                    }

                    if (request.Notifications.Count > 3)
                    {
                        // Report progress.
                        await progress.ShowProgress(_localization.Format("Move_Progress", request.Notifications.SafeCount(), category.Name));
                        await Task.Delay(750, cancellationToken);
                    }

                    foreach (var notification in request.Notifications)
                    {
                        var data = await context.GetNotificationQuery().FirstOrDefaultAsync(x => x.Id == notification.Id, cancellationToken);
                        if (data == null)
                        {
                            continue;
                        }

                        // Update the notification's category.
                        data.Category = category;
                        context.Update(data);
                    }

                    await context.SaveChangesAsync(cancellationToken);
                }

                // Notify subscribers of the change.
                await _messenger.PublishAsync(new NotificationMoved
                {
                    Category = request.Category,
                    Notifications = request.Notifications,
                });

                if (request.Notifications.Count > 3)
                {
                    // Wait a little bit.
                    await Task.Delay(500, cancellationToken);
                }
            }
        }

        private async Task<CategoryData> GetCategory(Request request, GhostlyContext context)
        {
            var category = await context.Categories.FindAsync(request.Category.Id);
            if (category == null)
            {
                _log.Warning("Cannot move notification to category {CategoryId} since it could not be found.", request.Category.Id);
                return null;
            }

            if (category.Kind == CategoryKind.Filter)
            {
                _log.Warning("Cannot move notification to category {CategoryId} since it's a filter.", request.Category.Id);
                return null;
            }

            return category;
        }
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Features.Activities.Payloads;
using Ghostly.Jobs;
using Ghostly.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Features.Notifications
{
    public sealed class ArchiveAllHandler : GhostlyRequestHandler<ArchiveAllHandler.Request>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMessageService _messenger;
        private readonly ILocalizer _localizer;
        private readonly IActivityQueue _queue;
        private readonly IGhostlyLog _log;

        public ArchiveAllHandler(
            IGhostlyContextFactory factory,
            IMessageService messenger,
            ILocalizer localizer,
            IActivityQueue queue,
            IGhostlyLog log)
        {
            _factory = factory;
            _messenger = messenger;
            _localizer = localizer;
            _queue = queue;
            _log = log;
        }

        public sealed class Request : IRequest
        {
            public Category Category { get; }

            public Request(Category category)
            {
                Category = category ?? throw new ArgumentNullException(nameof(category));
            }
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            using (var progress = new IndeterminateProgressReporter(_messenger))
            {
                using (var context = _factory.Create())
                {
                    var category = await context.Categories.FirstOrDefaultAsync(a => a.Archive, cancellationToken);
                    if (category == null)
                    {
                        _log.Error("Could not find archive category.");
                        return;
                    }

                    // Report progress.
                    await progress.ShowProgress(_localizer.Format("ArchiveAll_Progress", category.Name));
                    await Task.Delay(750, cancellationToken);

                    foreach (var notification in context.GetNotificationQuery().Where(x => x.Category.Id == request.Category.Id))
                    {
                        // Is the item unread?
                        if (notification.Unread)
                        {
                            await MarkAsRead(context, notification);
                        }

                        // Update the notification's category.
                        notification.Category = category;
                        context.Update(notification);
                    }

                    await context.SaveChangesAsync(cancellationToken);
                }

                // Notify subscribers of the change.
                await _messenger.PublishAsync(new RefreshNotifications());

                // Wait a little bit.
                await Task.Delay(500, cancellationToken);
            }
        }

        // TODO: Use handler or move this logic to a service so we can cache accounts...
        private async Task MarkAsRead(GhostlyContext context, NotificationData notification)
        {
            // Get the account.
            var accountData = await context.Accounts.FindAsync(notification.AccountId).ConfigureAwait(false);
            var account = AccountMapper.Map(accountData, _localizer);

            // Mark the notification as read.
            notification.Unread = false;

            // Queue up a new sync activity.
            _queue.Add(new MarkAsReadPayload
            {
                Vendor = account.VendorKind,
                NotificationId = notification.Id,
            });
        }
    }
}

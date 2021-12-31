using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

namespace Ghostly.Features.Notifications
{
    public sealed class MarkAllAsReadHandler : GhostlyRequestHandler<MarkAllAsReadHandler.Request>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMediator _mediator;
        private readonly IActivityQueue _queue;
        private readonly IMessageService _messenger;
        private readonly ILocalizer _localizer;

        public MarkAllAsReadHandler(
            IGhostlyContextFactory factory,
            IMediator mediator,
            IActivityQueue queue,
            IMessageService messenger,
            ILocalizer localizer)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        public sealed class Request : IRequest
        {
            public Category Category { get; }

            public Request(Category category)
            {
                Category = category ?? throw new ArgumentNullException(nameof(category));
            }

            public IEnumerable<NotificationData> GetNotifications(GhostlyContext context)
            {
                switch (Category.Kind)
                {
                    case CategoryKind.Category:
                        return context.GetNotificationQuery()
                            .Where(n => n.Category.Id == Category.Id)
                            .Where(n => n.Unread)
                            .ToList();
                    case CategoryKind.Filter:
                        return context.GetNotificationQuery()
                            .Where(Category.Filter)
                            .Where(n => n.Unread)
                            .ToList();
                    default:
                        throw new InvalidOperationException("Unknown category kind.");
                }
            }
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            using (var progress = new IndeterminateProgressReporter(_messenger))
            {
                // Report some progress.
                await progress.ShowProgress(_localizer.Format("MarkAllAsRead_Progress", request.Category.Name));
                await Task.Delay(500, cancellationToken);

                using (var context = _factory.Create())
                {
                    var notifications = request.GetNotifications(context).ToList();
                    foreach (var notification in notifications)
                    {
                        var account = await _mediator.GetAccount(notification.AccountId);
                        if (account != null)
                        {
                            notification.Unread = false;
                            context.Update(notification);

                            // Add a new synchronization activity.
                            _queue.Add(new MarkAsReadPayload
                            {
                                Vendor = account.VendorKind,
                                NotificationId = notification.Id,
                            });
                        }
                    }

                    // Save changes.
                    await context.SaveChangesAsync(cancellationToken);

                    // Tell subscribers what happened.
                    await _messenger.PublishAsync(new NotificationRead
                    {
                        Notifications = NotificationMapper.Map(notifications, _localizer),
                    });
                }
            }
        }
    }
}

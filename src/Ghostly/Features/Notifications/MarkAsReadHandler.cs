using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Features.Activities.Payloads;
using Ghostly.Jobs;
using Ghostly.Utilities;
using MediatR;

namespace Ghostly.Features.Notifications
{
    public sealed class MarkAsReadHandler : GhostlyRequestHandler<MarkAsReadHandler.Request>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMediator _mediator;
        private readonly IActivityQueue _queue;
        private readonly IMessageService _messenger;
        private readonly ILocalizer _localizer;

        public MarkAsReadHandler(
            IGhostlyContextFactory factory,
            IMediator mediator,
            IActivityQueue queue,
            IMessageService messenger,
            ILocalizer localizer)
        {
            _factory = factory ?? throw new System.ArgumentNullException(nameof(factory));
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _queue = queue ?? throw new System.ArgumentNullException(nameof(queue));
            _messenger = messenger ?? throw new System.ArgumentNullException(nameof(messenger));
            _localizer = localizer ?? throw new System.ArgumentNullException(nameof(localizer));
        }

        public sealed class Request : IRequest
        {
            public IReadOnlyList<Notification> Notifications { get; }
            public bool Broadcast { get; }
            public bool TellVendor { get; }

            public Request(IEnumerable<Notification> notifications, bool broadcast = true, bool tellVendor = true)
            {
                Notifications = notifications.ToReadOnlyList();
                Broadcast = broadcast;
                TellVendor = tellVendor;
            }
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            using (var progress = new IndeterminateProgressReporter(_messenger))
            {
                // Report some progress.
                await progress.ShowProgress(
                    request.Broadcast && request.Notifications.Count > 3,
                    _localizer.Format("MarkAsRead_Progress", request.Notifications.SafeCount()))
                        .ConfigureAwait(false);

                using (var context = _factory.Create())
                {
                    foreach (var notification in request.Notifications)
                    {
                        var account = await _mediator.GetAccount(notification.AccountId).ConfigureAwait(false);
                        if (account != null)
                        {
                            await MarkAsRead(notification, context).ConfigureAwait(false);

                            if (request.TellVendor)
                            {
                                // Add a new synchronization activity.
                                _queue.Add(new MarkAsReadPayload
                                {
                                    Vendor = account.VendorKind,
                                    NotificationId = notification.Id,
                                });
                            }
                        }
                    }

                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                if (request.Broadcast)
                {
                    // Tell subscribers what happened.
                    await _messenger.PublishAsync(new NotificationRead
                    {
                        Notifications = request.Notifications,
                    }).ConfigureAwait(false);
                }
            }
        }

        private static async Task MarkAsRead(Notification model, GhostlyContext context)
        {
            var notification = await context.Notifications.FindAsync(model.Id).ConfigureAwait(false);
            if (notification != null)
            {
                notification.Unread = false;
                context.Update(notification);
            }
        }
    }
}

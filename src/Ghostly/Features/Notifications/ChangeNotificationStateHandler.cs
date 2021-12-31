using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using MediatR;

namespace Ghostly.Features.Notifications
{
    public sealed class ChangeNotificationStateHandler : GhostlyRequestHandler<ChangeNotificationStateHandler.Request>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMessageService _messenger;

        public ChangeNotificationStateHandler(
            IGhostlyContextFactory factory,
            IMessageService messenger)
        {
            _factory = factory ?? throw new System.ArgumentNullException(nameof(factory));
            _messenger = messenger ?? throw new System.ArgumentNullException(nameof(messenger));
        }

        public sealed class Request : IRequest
        {
            public IReadOnlyList<Notification> Notifications { get; set; }
            public UpdateNotificationState NewState { get; set; }

            public Request(IEnumerable<Notification> notifications, UpdateNotificationState newState)
            {
                Notifications = notifications.ToReadOnlyList();
                NewState = newState;
            }
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                foreach (var model in request.Notifications)
                {
                    if (model.IsInState(request.NewState))
                    {
                        continue;
                    }

                    var notification = await context.Notifications.FindAsync(new object[] { model.Id }, cancellationToken);
                    if (notification != null)
                    {
                        if (request.NewState == UpdateNotificationState.Star)
                        {
                            model.Starred = true;
                            notification.Starred = true;
                        }

                        if (request.NewState == UpdateNotificationState.Unstar)
                        {
                            model.Starred = false;
                            notification.Starred = false;
                        }

                        if (request.NewState == UpdateNotificationState.Mute)
                        {
                            model.Muted = true;
                            model.Unread = false;
                            notification.Muted = true;
                        }

                        if (request.NewState == UpdateNotificationState.Unmute)
                        {
                            model.Muted = false;
                            model.Unread = notification.Unread;
                            notification.Muted = false;
                        }
                    }
                }

                await context.SaveChangesAsync(cancellationToken);
            }

            // Tell subscribers what happened.
            await _messenger.PublishAsync(new NotificationStateChanged
            {
                Notifications = request.Notifications.Select(n => n.Id).ToReadOnlyList(),
                State = request.NewState,
            });

            // Update muted/unmuted state.
            if (request.NewState == UpdateNotificationState.Mute)
            {
                await _messenger.PublishAsync(new NotificationMuted
                {
                    Notifications = request.Notifications,
                });
            }

            if (request.NewState == UpdateNotificationState.Unmute)
            {
                await _messenger.PublishAsync(new NotificationUnmuted
                {
                    Notifications = request.Notifications,
                });
            }

            // Update starred/unstarred state.
            if (request.NewState == UpdateNotificationState.Star)
            {
                await _messenger.PublishAsync(new NotificationStarred
                {
                    Notifications = request.Notifications,
                });
            }

            if (request.NewState == UpdateNotificationState.Unstar)
            {
                await _messenger.PublishAsync(new NotificationUnstarred
                {
                    Notifications = request.Notifications,
                });
            }
        }
    }
}

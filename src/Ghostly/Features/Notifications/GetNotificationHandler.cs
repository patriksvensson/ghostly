using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Features.Notifications
{
    public sealed class GetNotificationHandler : GhostlyRequestHandler<GetNotificationHandler.Request, Notification>
    {
        private readonly ILocalizer _localizer;

        public GetNotificationHandler(ILocalizer localizer)
        {
            _localizer = localizer;
        }

        public sealed class Request : IRequest<Notification>
        {
            public GhostlyContext Context { get; }
            public int NotificationId { get; }

            public Request(GhostlyContext context, int notificationId)
            {
                Context = context;
                NotificationId = notificationId;
            }
        }

        public override async Task<Notification> Handle(Request request, CancellationToken cancellationToken)
        {
            // Get the notification.
            var notification = await request.Context.GetNotificationQuery()
                .FirstOrDefaultAsync(x => x.Id == request.NotificationId, cancellationToken);

            if (notification == null)
            {
                return null;
            }

            return NotificationMapper.Map(notification, _localizer);
        }
    }
}

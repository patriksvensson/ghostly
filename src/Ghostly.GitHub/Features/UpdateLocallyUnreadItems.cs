using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;
using Ghostly.Domain.Messages;
using Ghostly.Features.Notifications;
using MediatR;

namespace Ghostly.GitHub.Actions
{
    internal sealed class UpdateLocallyUnreadItems : GitHubRequestHandler<UpdateLocallyUnreadItems.Request>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly ILocalSettings _settings;
        private readonly IMessageService _messenger;
        private readonly IMediator _mediator;
        private readonly ILocalizer _localizer;

        public UpdateLocallyUnreadItems(
            IGhostlyContextFactory factory,
            ILocalSettings settings,
            IMessageService messenger,
            IMediator mediator,
            ILocalizer localizer)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        public sealed class Request : IRequest<GitHubResult>
        {
            public IReadOnlyList<GitHubNotificationItem> UnreadItems { get; }

            public Request(IEnumerable<GitHubNotificationItem> unreadItems)
            {
                UnreadItems = unreadItems.ToReadOnlyList();
            }
        }

        protected override async Task<GitHubResult> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            if (!_settings.GetSetSynchronizeUnreadState())
            {
                return GitHubResult.Ok();
            }

            // Update status that the user sees since this might take a while.
            await _messenger.PublishAsync(new StatusMessage(
                _localizer.GetString("GitHub_Progress_Unread"), null));

            using (var context = _factory.Create())
            {
                var unread = NotificationMapper.Map(context.GetNotificationQuery()
                    .Where(x => x.Discriminator == Discriminator.GitHub && x.Unread), _localizer)
                    .OfType<GitHubNotification>()
                    .ToList();

                var result = new List<Notification>();
                foreach (var notification in unread)
                {
                    if (!request.UnreadItems.Any(n => n.Id == notification.GitHubId))
                    {
                        result.Add(notification);
                    }
                }

                if (result.Count > 0)
                {
                    await _mediator.Send(new MarkAsReadHandler.Request(
                        result,
                        broadcast: true,
                        tellVendor: false), cancellationToken);
                }
            }

            return GitHubResult.Ok();
        }
    }
}

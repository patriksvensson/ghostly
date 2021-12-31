using System;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Features.Notifications;
using Ghostly.GitHub.Octokit;
using Ghostly.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.GitHub.Actions
{
    internal sealed class UpdateGitHubNotification : GitHubRequestHandler<UpdateGitHubNotification.Request, UpdateGitHubNotification.Response>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly ICategoryService _category;
        private readonly IMediator _mediator;
        private readonly ILocalSettings _settings;
        private readonly IRuleService _rules;
        private readonly ITelemetry _telemetry;
        private readonly ILocalizer _localizer;
        private readonly IGhostlyLog _log;

        public UpdateGitHubNotification(
            IGhostlyContextFactory factory,
            ICategoryService category,
            IMediator mediator,
            ILocalSettings settings,
            IRuleService rules,
            ITelemetry telemetry,
            ILocalizer localizer,
            IGhostlyLog log)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _category = category ?? throw new ArgumentNullException(nameof(category));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public sealed class Request : IRequest<GitHubResult<Response>>
        {
            public IGitHubGateway Gateway { get; }
            public Account Account { get; }
            public GitHubNotificationItem Info { get; }
            public bool Force { get; }

            public Request(IGitHubGateway gateway, Account account, GitHubNotificationItem info, bool force)
            {
                Gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
                Account = account ?? throw new ArgumentNullException(nameof(account));
                Info = info ?? throw new ArgumentNullException(nameof(info));
                Force = force;
            }
        }

        public sealed class Response
        {
            public Notification Notification { get; set; }
            public bool WasUpdated { get; set; }
        }

        protected override async Task<GitHubResult<Response>> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            var gateway = request.Gateway;
            var account = request.Account;
            var info = request.Info;

            var wasUpdated = false;
            var workItemUpdated = false;

            using (var context = _factory.Create())
            {
                var category = await context.Categories.SingleOrDefaultAsync(x => x.Inbox, cancellationToken);
                if (category == null)
                {
                    throw new InvalidOperationException("Could not get default category.");
                }

                var notificationResult = await _mediator.Send(new GetGitHubNotification.Request(context, info.Id), cancellationToken);
                if (notificationResult.Faulted)
                {
                    return notificationResult.ForCaller(nameof(UpdateGitHubNotification))
                        .Track(_telemetry, "Could not get notification from database.")
                        .Log(_log, "Could not get notificiation {GitHubNotificationId} from database.", info.Id)
                        .GetResult().Convert<Response>();
                }

                var notification = notificationResult.Unwrap();
                if (notification == null)
                {
                    using (_log.Push("GitHubSyncAction", "Create"))
                    {
                        // Create the work item.
                        _log.Debug("Getting work item for notification.");
                        var workitemResult = await _mediator.UpdateGitHubWorkItem(gateway, context, info);
                        if (workitemResult.Faulted)
                        {
                            return workitemResult.ForCaller(nameof(UpdateGitHubNotification))
                                .Track(_telemetry, "Could not get work item for notification.")
                                .Log(_log, "Could not get work item for notification {GitHubNotificationId}", info.Id)
                                .GetResult().Convert<Response>();
                        }

                        var workitem = workitemResult.Unwrap();
                        if (workitem == null)
                        {
                            _log.Debug("Could not get work item for notification.");
                            return GitHubResult.Ok<Response>(null);
                        }

                        // Create the notification.
                        notification = new NotificationData
                        {
                            AccountId = account.Id,
                            Category = category,
                            Discriminator = Discriminator.GitHub,
                            GitHubId = info.Id,
                            WorkItem = workitem,
                            Unread = info.Unread,
                            Timestamp = info.UpdatedAt,
                            Reason = info.Reason,
                        };

                        wasUpdated = true;
                        context.Notifications.Add(notification);
                    }
                }
                else
                {
                    using (_log.Push("GitHubSyncAction", "Update"))
                    {
                        // Update the work item.
                        if (notification.Timestamp != info.UpdatedAt || request.Force)
                        {
                            _log.Debug("Updating work item for notification {GitHubNotificationId}.", info.Id);
                            var workitemResult = await _mediator.UpdateGitHubWorkItem(gateway, context, info);
                            if (workitemResult.Faulted)
                            {
                                return workitemResult.ForCaller(nameof(UpdateGitHubNotification))
                                    .Track(_telemetry, "Could not update work item.")
                                    .Log(_log, "Could not update work item for notification {GitHubNotificationId}.", info.Id)
                                    .GetResult().Convert<Response>();
                            }

                            if (workitemResult.Unwrap() == null)
                            {
                                _log.Warning("Could not update work item for notification.");
                                return GitHubResult.Ok<Response>(null);
                            }

                            workItemUpdated = true;
                        }

                        // Unread states haven't changed?
                        if (notification.Unread != info.Unread ||
                            notification.Timestamp != info.UpdatedAt)
                        {
                            _log.Debug("Updating notification with local id {LocalNotificationId}.", notification.Id);
                            notification.Unread = info.Unread;
                            notification.Timestamp = info.UpdatedAt;
                            notification.Reason = info.Reason;

                            wasUpdated = true;
                            context.Update(notification);
                        }
                    }
                }

                if (workItemUpdated || wasUpdated)
                {
                    // Save changes.
                    _log.Debug("Saving database changes related to synchronization.");
                    await context.SaveChangesAsync(true, cancellationToken);
                }

                if (wasUpdated)
                {
                    // Process the notification via the rule processor.
                    var processed = await _rules.Process(context, notification);
                    if (processed.Success)
                    {
                        notification = processed.Notification;
                        wasUpdated = true;
                    }
                }

                // Map the data to a proper notification.
                var mapped = NotificationMapper.Map(notification, _localizer);

                // Unread and muted?
                if (notification.Unread && notification.Muted)
                {
                    mapped.Unread = false;

                    if (_settings.GetMarkMutedAsRead())
                    {
                        await _mediator.Send(new MarkAsReadHandler.Request(
                                new[] { mapped },
                                broadcast: false,
                                tellVendor: true), cancellationToken);
                    }
                }

                // Return the notification.
                return GitHubResult.Ok(new Response
                {
                    Notification = mapped,
                    WasUpdated = wasUpdated,
                });
            }
        }
    }
}

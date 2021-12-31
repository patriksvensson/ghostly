using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Domain.GitHub;
using Ghostly.Domain.Messages;
using Ghostly.Features.Synchronization;
using Ghostly.GitHub.Actions;
using Ghostly.GitHub.Features;
using Ghostly.GitHub.Octokit;
using MediatR;

namespace Ghostly.GitHub.Synchronizers
{
    internal sealed class NotificationSynchronizer
    {
        private readonly GitHubSynchronizationQueue _queue;
        private readonly IMessageService _messenger;
        private readonly IToastNotifier _notifier;
        private readonly IMediator _mediator;
        private readonly ILocalSettings _settings;
        private readonly ITelemetry _telemetry;
        private readonly INetworkHelper _network;
        private readonly ILocalizer _localizer;
        private readonly IGhostlyLog _log;
        private readonly Random _random;

        public NotificationSynchronizer(
            GitHubSynchronizationQueue queue,
            IMessageService messenger,
            IToastNotifier notifier,
            IMediator mediator,
            ILocalSettings settings,
            ITelemetry telemetry,
            INetworkHelper network,
            ILocalizer localizer,
            IGhostlyLog log)
        {
            _queue = queue;
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _random = new Random(DateTime.Now.Millisecond);
        }

        public async Task<SynchronizationStatus> Synchronize(
            IGitHubGateway gateway,
            GitHubAccount account)
        {
            if (account.LastSyncedAt == null)
            {
                // Fist time we sync!
                await _messenger.PublishAsync(new InAppNotification(
                    _localizer.GetString("GitHub_FirstSynchronization"), TimeSpan.FromSeconds(15)));
            }

            // Get last days of notifications from GitHub.
            bool includePrivate = account.VendorKind == Domain.Vendor.GitHub;
            var syncPoint = account.LastSyncedAt?.EnsureUniversalTime();
            var responseResult = await _mediator.Send(new GetGitHubNotifications.Request(gateway, syncPoint, includePrivate));
            if (responseResult.Faulted)
            {
                if (responseResult.IsRateLimited)
                {
                    return responseResult.ForCaller(nameof(NotificationSynchronizer))
                        .Track(_telemetry, "Could not retrieve notifications since we were rate limited.")
                        .Log(_log, "Could not retrieve notifications since we were rate limited")
                        .WithResult(SynchronizationStatus.RateLimited);
                }

                return responseResult.ForCaller(nameof(NotificationSynchronizer))
                    .Track(_telemetry, "Could not retrieve notifications.")
                    .Log(_log, "Could not retrieve notifications.")
                    .WithResult(SynchronizationStatus.UnknownError);
            }

            var response = responseResult.Unwrap();
            if (response.Any())
            {
                // Dump the items to the log.
                _log.Verbose("Notifications received from GitHub: {GitHubNotificationIds}", response.Select(x => x.Id));
            }

            // Process
            var notifications = new HashSet<GitHubNotificationItem>(new GitHubNotificationItemComparer());
            var unreadNotifications = new HashSet<GitHubNotificationItem>(new GitHubNotificationItemComparer());
            foreach (var notification in response)
            {
                var item = GitHubNotificationItemMapper.Map(notification);
                if (item.Kind == GitHubWorkItemKind.Unknown)
                {
                    var message = "Skipping notification with subject {@NotificationSubject} since the kind was unknown.";
                    _log.Information(message, notification.Subject);
                    continue;
                }

                if (!_queue.Contains(item))
                {
                    notifications.Add(item);
                }

                if (item.Unread)
                {
                    unreadNotifications.Add(item);
                }
            }

            // Update unread items.
            var localUpdateResult = await _mediator.Send(new UpdateLocallyUnreadItems.Request(unreadNotifications));
            if (localUpdateResult.Faulted)
            {
                localUpdateResult.ForCaller(nameof(NotificationSynchronizer))
                    .Track(_telemetry, "Could not update locally unread items.")
                    .Log(_log, "Could not update locally unread items.");
            }

            if (notifications.Count > 0)
            {
                // Add all parsed notifications to the queue.
                _queue.Enqueue(notifications);
            }

            // Nothing to synchronize?
            if (_queue.Count == 0)
            {
                _log.Debug("Nothing to synchronize.");
                return SynchronizationStatus.Completed;
            }

            // Process notifications.
            _log.Debug("Processing {@NotificationCount} GitHub notifications.", notifications.Count);
            return await ProcessNotifications(gateway, account);
        }

        public async Task<SynchronizationStatus> Synchronize(
            IGitHubGateway gateway,
            GitHubAccount account,
            GitHubNotification notification)
        {
            using (_log.Push("NotificationGitHubId", notification.Id))
            {
                try
                {
                    var remoteNotification = await _mediator.Send(new GetRemoteGitHubNotification.Request(gateway, notification.GitHubId));
                    if (remoteNotification.Faulted)
                    {
                        remoteNotification.ForCaller(nameof(NotificationSynchronizer))
                            .Track(_telemetry, "Could not update GitHub notification.")
                            .Log(_log, "Could not update notification {GitHubNotificationId}.", notification.Id);
                        return SynchronizationStatus.UnknownError;
                    }

                    var foo = GitHubNotificationItemMapper.Map(remoteNotification.Unwrap());
                    var itemResult = await _mediator.UpdateGitHubNotification(gateway, account, foo, true);
                    if (itemResult.Faulted)
                    {
                        itemResult.ForCaller(nameof(NotificationSynchronizer))
                            .Track(_telemetry, "Could not update GitHub notification.")
                            .Log(_log, "Could not update notification {GitHubNotificationId}.", notification.Id);

                        // If we're rate limited, then we should abort.
                        if (itemResult.IsRateLimited)
                        {
                            return SynchronizationStatus.RateLimited;
                        }

                        // Lost network connection? Then abort as well.
                        if (!_network.IsConnected)
                        {
                            return SynchronizationStatus.NoInternetConnection;
                        }

                        // Contine with the next item.
                        return SynchronizationStatus.UnknownError;
                    }

                    var item = itemResult.Unwrap();
                    if (item != null && item.WasUpdated)
                    {
                        await _messenger.PublishAsync(new NewNotifications(new[] { item.Notification }));
                    }

                    return SynchronizationStatus.Completed;
                }
                catch (Exception ex)
                {
                    // This should not happen, but let's catch it anyway.
                    _log.Error(ex);
                    _telemetry.TrackException(ex, nameof(NotificationSynchronizer));
                    return SynchronizationStatus.UnknownError;
                }
            }
        }

        private async Task<SynchronizationStatus> ProcessNotifications(
            IGitHubGateway gateway, GitHubAccount account)
        {
            var result = new List<Domain.Notification>();

            var count = Math.Min(_queue.Count, 100); // Limit how many notifications to sync at a time.
            var total = _queue.Count;
            var processed = 0;

            await _messenger.PublishAsync(
                new StatusMessage(
                    _localizer.Format("GitHub_Progress", count, total),
                    _queue.Count > 0 ? 0 : (int?)null));

            while (true)
            {
                if (!_queue.TryDequeue(5, out var notifications))
                {
                    break;
                }

                var models = new List<Domain.Notification>();
                foreach (var notification in notifications)
                {
                    using (_log.Push("NotificationGitHubId", notification.Id))
                    {
                        try
                        {
                            var itemResult = await _mediator.UpdateGitHubNotification(gateway, account, notification);
                            if (itemResult.Faulted)
                            {
                                itemResult.ForCaller(nameof(NotificationSynchronizer))
                                    .Track(_telemetry, "Could not update GitHub notification.")
                                    .Log(_log, "Could not update notification {GitHubNotificationId}.", notification.Id);

                                // If we're rate limited, then we should abort.
                                if (itemResult.IsRateLimited)
                                {
                                    return SynchronizationStatus.RateLimited;
                                }

                                // Lost network connection? Then abort as well.
                                if (!_network.IsConnected)
                                {
                                    return SynchronizationStatus.NoInternetConnection;
                                }

                                // Contine with the next item.
                                continue;
                            }

                            var item = itemResult.Unwrap();
                            if (item != null && item.WasUpdated)
                            {
                                models.Add(item.Notification);
                            }
                        }
                        catch (Exception ex)
                        {
                            // This should not happen, but let's catch it anyway.
                            _log.Error(ex);
                            _telemetry.TrackException(ex, nameof(NotificationSynchronizer));
                            continue;
                        }

                        processed += 1;
                    }

                    // Don't update all the time, that makes it feel slower somehow. ¯\_(ツ)_/¯
                    if (_random.Next(0, 100) % 2 == 0)
                    {
                        // Update progress
                        await _messenger.PublishAsync(
                            new StatusMessage(_localizer.Format("GitHub_Progress", count, total),
                            (int)(((double)processed / (double)count) * 100D)));
                    }
                }

                // Update progress
                await _messenger.PublishAsync(
                    new StatusMessage(_localizer.Format("GitHub_Progress", count, total),
                    (int)(((double)processed / (double)count) * 100D)));

                if (models.Count > 0)
                {
                    // Keep the processed items around.
                    result.AddRange(models);

                    // Send notifications for this batch.
                    _log.Information("Refreshing {NotificationCount} GitHub notifications.", models.Count);
                    await _messenger.PublishAsync(new NewNotifications(models));
                }

                // Got as many items that we need?
                if (processed >= count)
                {
                    break;
                }
            }

            // Show a toast telling the user of new, unread, non muted notifications.
            result = result.Where(x => !x.Muted && x.Unread).ToList();
            if (result.Count > 0 && _settings.GetShowNotificationToasts())
            {
                if (result.Count <= 3 && !_settings.GetAggregateNotificationToasts())
                {
                    foreach (var item in result)
                    {
                        _notifier.Show(
                            _localizer.GetString("GitHub_Toast_NewNotification_Title"),
                            item?.Title ?? "N/A");
                    }
                }
                else
                {
                    _notifier.Show(
                        _localizer.GetString("GitHub_Toast_NewNotifications_Title"),
                        _localizer.Format("GitHub_Toast_NewNotifications_Body", result.SafeCount()));
                }
            }

            // Synchronization completed.
            return SynchronizationStatus.Completed;
        }
    }
}

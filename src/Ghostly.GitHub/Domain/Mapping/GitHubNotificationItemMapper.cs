using System;
using System.Globalization;
using OctokitNotification = global::Octokit.Notification;
using OctokitNotificationSubject = global::Octokit.NotificationInfo;
using OctokitRepository = global::Octokit.Repository;

namespace Ghostly.GitHub
{
    internal static class GitHubNotificationItemMapper
    {
        public static GitHubNotificationItem Map(OctokitNotification notification)
        {
            var item = new GitHubNotificationItem();

            item.Id = long.Parse(notification.Id, CultureInfo.InvariantCulture);
            item.Url = notification.Url;
            item.Reason = notification.Reason;
            item.Unread = notification.Unread;

            item.Kind = notification.ParseKind();
            if (notification.TryGetId(item.Kind, out var workItemId))
            {
                item.WorkItemId = workItemId;
            }

            item.UpdatedAt = DateTime.Parse(notification.UpdatedAt, null, DateTimeStyles.RoundtripKind);
            if (notification.LastReadAt != null)
            {
                item.LastReadAt = DateTime.Parse(notification.LastReadAt, null, DateTimeStyles.RoundtripKind);
            }

            item.Subject = ParseSubject(notification.Subject);
            item.Repository = ParseRepository(notification.Repository);

            return item;
        }

        private static GitHubNotificationSubject ParseSubject(OctokitNotificationSubject subject)
        {
            if (subject == null)
            {
                return null;
            }

            return new GitHubNotificationSubject
            {
                Title = subject.Title,
                Url = subject.Url,
                LatestCommentUrl = subject.LatestCommentUrl,
                Type = subject.Type,
            };
        }

        private static GitHubRepositoryInfo ParseRepository(OctokitRepository repository)
        {
            if (repository == null)
            {
                return null;
            }

            return new GitHubRepositoryInfo
            {
                Id = repository.Id,
                Owner = repository.Owner?.Login,
                Name = repository.Name,
                HtmlUrl = repository.HtmlUrl,
                IsPrivate = repository.Private,
                IsFork = repository.Fork,
            };
        }
    }
}

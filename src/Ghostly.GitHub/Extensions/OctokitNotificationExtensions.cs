using System;
using System.Globalization;
using System.Linq;
using Ghostly.Domain.GitHub;
using OctokitNotification = Octokit.Notification;

namespace Ghostly.GitHub
{
    internal static class OctokitNotificationExtensions
    {
        public static GitHubWorkItemKind ParseKind(this OctokitNotification notification)
        {
            if (IsIssueNotification(notification))
            {
                return GitHubWorkItemKind.Issue;
            }
            else if (IsPullRequestNotification(notification))
            {
                return GitHubWorkItemKind.PullRequest;
            }
            else if (IsReleaseNotification(notification))
            {
                return GitHubWorkItemKind.Release;
            }
            else if (IsRepositoryVulnerabilityAlert(notification))
            {
                return GitHubWorkItemKind.Vulnerability;
            }
            else if (IsCommit(notification))
            {
                return GitHubWorkItemKind.Commit;
            }
            else if (IsDiscussion(notification))
            {
                return GitHubWorkItemKind.Discussion;
            }

            return GitHubWorkItemKind.Unknown;
        }

        public static bool IsIssueNotification(this OctokitNotification notification)
        {
            return notification.Subject.Type.Equals("Issue", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsPullRequestNotification(this OctokitNotification notification)
        {
            return notification.Subject.Type.Equals("PullRequest", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsReleaseNotification(this OctokitNotification notification)
        {
            return notification.Subject.Type.Equals("Release", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsRepositoryVulnerabilityAlert(this OctokitNotification notification)
        {
            return notification.Subject.Type.Equals("RepositoryVulnerabilityAlert", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCommit(this OctokitNotification notification)
        {
            return notification.Subject.Type.Equals("Commit", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDiscussion(this OctokitNotification notification)
        {
            return notification.Subject.Type.Equals("Discussion", StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryGetId(this OctokitNotification notification, GitHubWorkItemKind kind, out long id)
        {
            if (kind == GitHubWorkItemKind.Vulnerability ||
                kind == GitHubWorkItemKind.Commit ||
                kind == GitHubWorkItemKind.Discussion)
            {
                // Use the issue ID as the ID.
                id = long.Parse(notification.Id, CultureInfo.InvariantCulture);
                return true;
            }

            if (Uri.TryCreate(notification.Subject.Url, UriKind.Absolute, out var uri))
            {
                if (long.TryParse(uri.Segments.Last(), out long issueId))
                {
                    id = issueId;
                    return true;
                }
            }

            id = 0;
            return false;
        }
    }
}

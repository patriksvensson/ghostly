using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghostly.GitHub.Octokit;
using OctokitComment = Octokit.IssueComment;
using OctokitIssue = Octokit.Issue;
using OctokitNotification = Octokit.Notification;
using OctokitNotificationRequest = Octokit.NotificationsRequest;
using OctokitPullRequest = Octokit.PullRequest;
using OctokitRateLimit = Octokit.MiscellaneousRateLimit;
using OctokitRelease = Octokit.Release;
using OctokitReview = Octokit.PullRequestReview;
using OctokitReviewComment = Octokit.PullRequestReviewComment;
using OctokitUser = Octokit.User;

namespace Ghostly.GitHub
{
    internal static class GitHubGatewayExtensions
    {
        public static async Task<GitHubResult<OctokitIssue>> GetIssue(this IGitHubGateway gateway, string owner, string repository, long id)
        {
            return await gateway.Execute(
                client => client.Issue.Get(owner, repository, id));
        }

        public static async Task<GitHubResult<OctokitPullRequest>> GetPullRequest(this IGitHubGateway gateway, string owner, string repository, long id)
        {
            return await gateway.Execute(
                client => client.PullRequest.Get(owner, repository, id));
        }

        public static async Task<GitHubResult<OctokitRelease>> GetRelease(this IGitHubGateway gateway, string owner, string repository, long id)
        {
            return await gateway.Execute(
                client => client.Repository.Release.Get(owner, repository, id));
        }

        public static async Task<bool> IsAuthorized(this IGitHubGateway gateway)
        {
            try
            {
                var result = await gateway.Execute(client => client.User.Current());
                return !result.Faulted || !(result?.Exception is global::Octokit.AuthorizationException);
            }
            catch (global::Octokit.AuthorizationException)
            {
                // Should probably not happen, but let's check this anyway.
                return false;
            }
        }

        public static async Task<GitHubResult<OctokitRateLimit>> GetRateLimits(this IGitHubGateway gateway)
        {
            return await gateway.Execute(
                client => client.Miscellaneous.GetRateLimits());
        }

        public static async Task<GitHubResult<IReadOnlyList<OctokitNotification>>> GetUnreadNotifications(this IGitHubGateway gateway)
        {
            return await gateway.Execute(
                client => client.Activity.Notifications.GetAllForCurrent(new OctokitNotificationRequest()));
        }

        public static async Task<GitHubResult<IReadOnlyList<OctokitNotification>>> GetAllNotifications(this IGitHubGateway gateway, DateTime? timestamp)
        {
            return await gateway.Execute(
                client => client.Activity.Notifications.GetAllForCurrent(new OctokitNotificationRequest
                {
                    All = true,
                    Since = timestamp ?? DateTime.UtcNow.AddDays(-14),
                }));
        }

        public static async Task<GitHubResult<IReadOnlyList<OctokitComment>>> GetIssueComments(this IGitHubGateway gateway, string owner, string repository, long id)
        {
            return await gateway.Execute(
                client => client.Issue.Comment.GetAllForIssue(owner, repository, (int)id));
        }

        public static async Task<GitHubResult> MarkAsRead(this IGitHubGateway gateway, long id)
        {
            return await gateway.Execute(client => client.Activity.Notifications.MarkAsRead(id));
        }

        public static async Task<GitHubResult<OctokitUser>> GetUser(this IGitHubGateway gateway, string username)
        {
            return await gateway.Execute(client => client.User.Get(username));
        }

        public static async Task<GitHubResult<IReadOnlyList<OctokitReview>>> GetReviews(this IGitHubGateway gateway, string owner, string repository, int number)
        {
            return await gateway.Execute(client => client.PullRequest.Review.GetAll(owner, repository, number));
        }

        public static async Task<GitHubResult<IReadOnlyList<OctokitReviewComment>>> GetReviewComments(this IGitHubGateway gateway, string owner, string repository, int number)
        {
            return await gateway.Execute(client => client.PullRequest.ReviewComment.GetAll(owner, repository, number));
        }
    }
}

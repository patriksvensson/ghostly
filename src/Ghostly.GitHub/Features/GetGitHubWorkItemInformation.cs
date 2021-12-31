using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Data.Models;
using Ghostly.Domain.GitHub;
using Ghostly.GitHub.Octokit;
using MediatR;
using OctokitItemState = Octokit.ItemState;
using OctokitNotFoundException = Octokit.NotFoundException;

namespace Ghostly.GitHub.Actions
{
    internal sealed class GetGitHubWorkItemInformation : GitHubRequestHandler<GetGitHubWorkItemInformation.Request, GitHubWorkItemInfo>
    {
        private readonly ITelemetry _telemetry;
        private readonly IGhostlyLog _log;

        public sealed class Request : IRequest<GitHubResult<GitHubWorkItemInfo>>
        {
            public IGitHubGateway GitHub { get; }
            public GitHubNotificationItem Info { get; }

            public Request(IGitHubGateway github, GitHubNotificationItem info)
            {
                GitHub = github ?? throw new ArgumentNullException(nameof(github));
                Info = info ?? throw new ArgumentNullException(nameof(info));
            }
        }

        public GetGitHubWorkItemInformation(ITelemetry telemetry, IGhostlyLog log)
        {
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _log = log ?? throw new ArgumentNullException(nameof(telemetry));
        }

        protected override async Task<GitHubResult<GitHubWorkItemInfo>> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            var info = request.Info;

            // TODO: Move validation
            if (info.Repository == null)
            {
                _telemetry.TrackAndLogError(_log, "Can't get information about notification since it's missing repository.");
                return GitHubResult.Ok<GitHubWorkItemInfo>();
            }

            if (string.IsNullOrWhiteSpace(info.Repository.Name))
            {
                _telemetry.TrackAndLogError(_log, "Can't get information about notification since repository name is empty.");
                return GitHubResult.Ok<GitHubWorkItemInfo>();
            }

            if (info.Repository.Owner == null)
            {
                _telemetry.TrackAndLogError(_log, "Can't get information about notification since it's missing repository owner.");
                return GitHubResult.Ok<GitHubWorkItemInfo>();
            }

            if (string.IsNullOrWhiteSpace(info.Repository.Owner))
            {
                _telemetry.TrackAndLogError(_log, "Can't get information about notification since repository owner is empty.");
                return GitHubResult.Ok<GitHubWorkItemInfo>();
            }

            var owner = info.Repository.Owner;
            var repository = info.Repository.Name;

            if (info.Kind == GitHubWorkItemKind.Issue)
            {
                return await GetIssue(request, info, owner, repository);
            }
            else if (info.Kind == GitHubWorkItemKind.PullRequest)
            {
                return await GetPullRequest(request, info, owner, repository);
            }
            else if (info.Kind == GitHubWorkItemKind.Release)
            {
                return await GetRelease(request, info, owner, repository);
            }
            else if (info.Kind == GitHubWorkItemKind.Vulnerability)
            {
                return GetVulnerability(info);
            }
            else if (info.Kind == GitHubWorkItemKind.Commit)
            {
                return await GetCommit(request, info);
            }
            else if (info.Kind == GitHubWorkItemKind.Discussion)
            {
                return GetDiscussion(info);
            }
            else
            {
                throw new InvalidOperationException($"Unknown notification type {info.Kind}.");
            }
        }

        private async Task<GitHubResult<GitHubWorkItemInfo>> GetIssue(Request request, GitHubNotificationItem info, string owner, string repository)
        {
            var issueResult = await request.GitHub.GetIssue(owner, repository, info.WorkItemId);
            if (issueResult.Faulted)
            {
                return issueResult.ForCaller(nameof(GetGitHubWorkItemInformation))
                    .Track(_telemetry, "Could not retrieve issue.")
                    .Log(_log, "Could not retrieve issue {GitHubWorkItemId} for repository {GitHubRepositoryOrg}/{GitHubRepositoryName}",
                        info.WorkItemId, owner, repository)
                    .GetResult().Convert<GitHubWorkItemInfo>();
            }

            var issue = issueResult.Unwrap();

            return GitHubResult.Ok(new GitHubWorkItemInfo
            {
                GitHubId = issue.Id,
                GitHubRepositoryId = info.Repository.Id,
                LocalId = issue.Number,
                Title = issue.Title,
                Body = issue.Body,
                Url = issue.HtmlUrl,
                User = issue.User.Login,
                State = GetWorkItemState(issue, i => i.State.Value, i => i.ClosedAt),
                Locked = issue.Locked,
                CreatedAt = issue.CreatedAt.UtcDateTime,
                UpdatedAt = issue.UpdatedAt?.UtcDateTime,
                Assignees = issue.Assignees?.Select(x => x.Login)?.ToList(),
                Milestone = issue.Milestone == null ? null : new GitHubMilestoneInfo
                {
                    Id = issue.Milestone.Id,
                    Name = issue.Milestone.Title,
                },
                Labels = issue.Labels?.Select(l =>
                {
                    return new GitHubLabelInfo
                    {
                        Id = l.Id,
                        Owner = owner,
                        Repository = repository,
                        Name = l.Name,
                        Description = l.Description,
                        Color = l.Color,
                    };
                })?.ToList() ?? new List<GitHubLabelInfo>(),
            });
        }

        private async Task<GitHubResult<GitHubWorkItemInfo>> GetPullRequest(Request request, GitHubNotificationItem info, string owner, string repository)
        {
            var pullrequestResult = await request.GitHub.GetPullRequest(owner, repository, info.WorkItemId);
            if (pullrequestResult.Faulted)
            {
                // Deleted pull request
                if (pullrequestResult.Exception is OctokitNotFoundException ex)
                {
                    return GitHubResult.Ok(new GitHubWorkItemInfo
                    {
                        GitHubId = info.WorkItemId,
                        Title = info.Subject.Title,
                        Deleted = true,
                        CreatedAt = info.UpdatedAt,
                    });
                }

                return pullrequestResult.ForCaller(nameof(GetGitHubWorkItemInformation))
                    .Track(_telemetry, "Could not retrieve pull request.")
                    .Log(_log, "Could not retrieve pull request {GitHubWorkItemId} for repository {GitHubRepositoryOrg}/{GitHubRepositoryName}",
                        info.WorkItemId, owner, repository)
                    .GetResult().Convert<GitHubWorkItemInfo>();
            }

            var pullrequest = pullrequestResult.Unwrap();
            if (pullrequest.User == null)
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize pull request since it's missing a user.");
                return GitHubResult.Ok<GitHubWorkItemInfo>();
            }

            if (string.IsNullOrWhiteSpace(pullrequest.User.Login))
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize pull request since the user login is empty.");
                return GitHubResult.Ok<GitHubWorkItemInfo>();
            }

            return GitHubResult.Ok(new GitHubWorkItemInfo
            {
                GitHubId = pullrequest.Id,
                GitHubRepositoryId = info.Repository.Id,
                LocalId = pullrequest.Number,
                Title = pullrequest.Title,
                Body = pullrequest.Body,
                Url = pullrequest.HtmlUrl,
                User = pullrequest.User.Login,
                State = GetWorkItemState(pullrequest, i => i.State.Value, i => i.ClosedAt),
                Merged = pullrequest.Merged,
                MergedBy = pullrequest.MergedBy?.Login,
                Locked = pullrequest.Locked,
                Draft = pullrequest.Draft,
                CreatedAt = pullrequest.CreatedAt.UtcDateTime,
                UpdatedAt = pullrequest.UpdatedAt.UtcDateTime,
                MergedAt = pullrequest.MergedAt?.UtcDateTime,
                Assignees = pullrequest.Assignees?.Select(x => x.Login).ToList(),
                Milestone = pullrequest.Milestone == null ? null : new GitHubMilestoneInfo
                {
                    Id = pullrequest.Milestone.Id,
                    Name = pullrequest.Milestone.Title,
                },
                Labels = pullrequest.Labels?.Select(l =>
                {
                    return new GitHubLabelInfo
                    {
                        Id = l.Id,
                        Owner = owner,
                        Repository = repository,
                        Name = l.Name,
                        Description = l.Description,
                        Color = l.Color,
                    };
                })?.ToList(),
            });
        }

        private async Task<GitHubResult<GitHubWorkItemInfo>> GetRelease(Request request, GitHubNotificationItem info, string owner, string repository)
        {
            var releaseResult = await request.GitHub.GetRelease(owner, repository, info.WorkItemId);
            if (releaseResult.Faulted)
            {
                return releaseResult.ForCaller(nameof(GetGitHubWorkItemInformation))
                    .Track(_telemetry, "Could not retrieve release.")
                    .Log(_log, "Could not retrieve release {GitHubWorkItemId} for repository {GitHubRepositoryOrg}/{GitHubRepositoryName}",
                        info.WorkItemId, owner, repository)
                    .GetResult().Convert<GitHubWorkItemInfo>();
            }

            var release = releaseResult.Unwrap();
            if (release.Author == null)
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize release since it's missing a user.");
                return GitHubResult.Ok<GitHubWorkItemInfo>();
            }

            if (string.IsNullOrWhiteSpace(release.Author.Login))
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize release since the user login is empty.");
                return GitHubResult.Ok<GitHubWorkItemInfo>();
            }

            return GitHubResult.Ok(new GitHubWorkItemInfo
            {
                GitHubId = release.Id,
                GitHubRepositoryId = info.Repository.Id,
                Title = release.Name,
                Body = release.Body,
                Url = release.HtmlUrl,
                User = release.Author.Login,
                State = WorkItemState.Open,
                CreatedAt = release.CreatedAt.UtcDateTime,
                UpdatedAt = release.PublishedAt?.UtcDateTime,
            });
        }

        private static GitHubResult<GitHubWorkItemInfo> GetVulnerability(GitHubNotificationItem info)
        {
            return GitHubResult.Ok(new GitHubWorkItemInfo
            {
                GitHubId = info.Id,
                GitHubRepositoryId = info.Repository.Id,
                Title = info.Subject.Title,
                Body = info.Subject.Title,
                CreatedAt = info.UpdatedAt,
                UpdatedAt = info.UpdatedAt,
                State = WorkItemState.Open,
                User = null,
                Url = info.Repository.HtmlUrl,
            });
        }

        private static GitHubResult<GitHubWorkItemInfo> GetDiscussion(GitHubNotificationItem info)
        {
            return GitHubResult.Ok(new GitHubWorkItemInfo
            {
                GitHubId = info.Id,
                GitHubRepositoryId = info.Repository.Id,
                Title = info.Subject.Title,
                Body = info.Subject.Title,
                CreatedAt = info.UpdatedAt,
                UpdatedAt = info.UpdatedAt,
                State = WorkItemState.Open,
                User = null,
                Url = info.Repository.HtmlUrl + "/discussions",
            });
        }

        private async Task<GitHubResult<GitHubWorkItemInfo>> GetCommit(Request request, GitHubNotificationItem info)
        {
            if (!info.Subject.LatestCommentUrl.TryGetId(out var commitId))
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize commit since we could not get the commit ID.");
                return GitHubResult.Ok<GitHubWorkItemInfo>();
            }

            if (!info.Subject.Url.TryGetLastSegment(out var sha))
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize commit since we could not get the commit SHA.");
                return GitHubResult.Ok<GitHubWorkItemInfo>();
            }

            // Get the commit.
            var commitResult = await request.GitHub.Execute(c => c.Repository.Commit.Get(info.Repository.Id, sha));
            if (commitResult.Faulted)
            {
                return commitResult.ForCaller(nameof(GetGitHubWorkItemInformation))
                    .Track(_telemetry, "Could not retrieve commit.")
                    .Log(_log, "Could not retrieve commit {CommitSha} for repository {GitHubRepositoryId}",
                        sha, info.Repository.Id)
                    .GetResult().Convert<GitHubWorkItemInfo>();
            }

            var commit = commitResult.Unwrap();

            if (commit.Author == null)
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize commit since it's missing a user.");
                return GitHubResult.Ok<GitHubWorkItemInfo>();
            }

            if (string.IsNullOrWhiteSpace(commit.Author.Login))
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize commit since the user login is empty.");
                return GitHubResult.Ok<GitHubWorkItemInfo>();
            }

            return GitHubResult.Ok(new GitHubWorkItemInfo
            {
                GitHubId = info.Id,
                GitHubRepositoryId = info.Repository.Id,
                CommitId = commitId,
                CommitSha = sha,
                Title = info.Subject.Title,
                Body = info.Subject.Title,
                CreatedAt = info.UpdatedAt,
                UpdatedAt = info.UpdatedAt,
                State = WorkItemState.Open,
                User = commit.Author.Login,
                Url = commit.HtmlUrl,
            });
        }

        private static WorkItemState GetWorkItemState<T>(T item, Func<T, OctokitItemState> stateGetter, Func<T, DateTimeOffset?> closedGetter)
        {
            var wasClosed = closedGetter(item) != null;
            var openState = stateGetter(item) == OctokitItemState.Open ? WorkItemState.Open : WorkItemState.Closed;
            var finalOpenState = wasClosed && openState == WorkItemState.Open ? WorkItemState.Reopened : openState;
            return finalOpenState;
        }
    }
}

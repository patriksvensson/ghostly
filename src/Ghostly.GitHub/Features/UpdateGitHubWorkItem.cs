using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Domain.GitHub;
using Ghostly.GitHub.Octokit;
using MediatR;

namespace Ghostly.GitHub.Actions
{
    internal sealed class UpdateGitHubWorkItem : GitHubRequestHandler<UpdateGitHubWorkItem.Request, WorkItemData>
    {
        private readonly IMediator _mediator;
        private readonly ITelemetry _telemetry;
        private readonly IGhostlyLog _log;

        public UpdateGitHubWorkItem(
            IMediator mediator,
            ITelemetry telemetry,
            IGhostlyLog log)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public sealed class Request : IRequest<GitHubResult<WorkItemData>>
        {
            public IGitHubGateway Gateway { get; }
            public GhostlyContext Context { get; }
            public GitHubNotificationItem Info { get; }

            public Request(IGitHubGateway gateway, GhostlyContext context, GitHubNotificationItem info)
            {
                Gateway = gateway;
                Context = context;
                Info = info;
            }
        }

        protected override async Task<GitHubResult<WorkItemData>> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            // Get the repository information.
            var repositoryId = request.Info.Repository.Id;
            var owner = request.Info.Repository.Owner;
            var repoName = request.Info.Repository.Name;

            // Get the work item.
            var workitemResult = await _mediator.GetGitHubWorkItem(
                request.Context, request.Info.Kind,
                request.Info.WorkItemId, request.Info.Repository.Id);

            if (workitemResult.Faulted)
            {
                return workitemResult.ForCaller(nameof(UpdateGitHubWorkItem))
                    .Track(_telemetry, "Could not get work item from database.")
                    .Log(_log, "Could not retrieve work item {GitHubWorkItemId} from database.", request.Info.WorkItemId)
                    .GetResult().Convert<WorkItemData>();
            }

            // Download the work item information from GitHub.
            var infoResult = await _mediator.GetGitHubWorkItemInformation(request.Gateway, request.Info);
            if (infoResult.Faulted)
            {
                return infoResult.ForCaller(nameof(UpdateGitHubWorkItem))
                    .Track(_telemetry, "Could not update work item.")
                    .Log(_log, "Could not update work item. (Kind={WorkItemKind}, Id={WorkItemId})", request.Info.Kind, request.Info.WorkItemId)
                    .GetResult().Convert<WorkItemData>();
            }

            var info = infoResult.Unwrap();
            if (info == null)
            {
                _log.Error("Could not retrieve new data for work item #{GitHubWorkItemId}.", request.Info.WorkItemId);
                return GitHubResult.Ok<WorkItemData>(null);
            }

            if (string.IsNullOrWhiteSpace(info.User) &&
                request.Info.Kind.RequiresAuthor())
            {
                _log.Error("Can not update work item since it requires an author which has not been specified.");
                return GitHubResult.Ok<WorkItemData>(null);
            }

            var workitem = workitemResult.Unwrap();
            if (workitem == null)
            {
                // Was the work item deleted?
                if (info.Deleted)
                {
                    _log.Information("Work item for notification #{GitHubWorkItemId} have been deleted and will automatically be marked as read.", request.Info.Id);
                    var markAsReadResult = await request.Gateway.MarkAsRead((int)request.Info.Id);
                    if (markAsReadResult.Faulted)
                    {
                        markAsReadResult.ForCaller(nameof(UpdateGitHubWorkItem))
                            .Track(_telemetry, "Could not mark deleted notification as read.")
                            .Log(_log, "Could not mark deleted notification as read.");
                    }

                    return markAsReadResult.Convert<WorkItemData>();
                }

                var author = default(UserData);
                if (!string.IsNullOrWhiteSpace(info.User))
                {
                    var authorResult = await _mediator.Send(new GetGitHubUser.Request(request.Gateway, request.Context, info.User, true), cancellationToken);
                    if (authorResult.Faulted)
                    {
                        return authorResult.ForCaller(nameof(UpdateGitHubWorkItem))
                            .Track(_telemetry, "Could not retrieve user.")
                            .Log(_log, "Could not retrieve user @{GitHubLogin} for item #{GitHubWorkItemId}", info.User, request.Info.WorkItemId)
                            .GetResult().Convert<WorkItemData>();
                    }

                    author = authorResult.Unwrap();
                }

                // Get the repository.
                var repositoryResult = await _mediator.GetGitHubRepository(request.Context, repositoryId);
                if (repositoryResult.Faulted)
                {
                    return repositoryResult.ForCaller(nameof(UpdateGitHubWorkItem))
                        .Track(_telemetry, "Could not retrieve repository from database.")
                        .Log(_log, "Could not retrieve repository with ID {RepositoryGitHubId} from database.", repositoryId)
                        .GetResult().Convert<WorkItemData>();
                }

                var repository = repositoryResult.Unwrap();
                if (repository == null)
                {
                    repository = new RepositoryData
                    {
                        Discriminator = Discriminator.GitHub,
                        GitHubId = repositoryId,
                        Private = request.Info.Repository.IsPrivate,
                        Fork = request.Info.Repository.IsFork,
                        Owner = owner,
                        Name = repoName,
                    };
                }

                // Create the work item.
                workitem = new WorkItemData
                {
                    IsPullRequest = request.Info.Kind == GitHubWorkItemKind.PullRequest,
                    IsIssue = request.Info.Kind == GitHubWorkItemKind.Issue,
                    IsRelease = request.Info.Kind == GitHubWorkItemKind.Release,
                    IsVulnerability = request.Info.Kind == GitHubWorkItemKind.Vulnerability,
                    IsDiscussion = request.Info.Kind == GitHubWorkItemKind.Discussion,
                    IsCommit = request.Info.Kind == GitHubWorkItemKind.Commit,
                    CommitSha = info.CommitSha,
                    Merged = info.Merged,
                    MergedAt = info.MergedAt,
                    Locked = info.Locked,
                    IsDraft = info.Draft,
                    Url = info.Url,
                    CreatedAt = info.CreatedAt,
                    UpdatedAt = info.UpdatedAt,
                    Title = info.Title,
                    Preamble = GeneratePreamble(info),
                    Body = info.Body,
                    IsOpen = info.State == WorkItemState.Open,
                    IsClosed = info.State == WorkItemState.Closed,
                    IsReopened = info.State == WorkItemState.Reopened,
                    GitHubId = info.GitHubId,
                    Author = author,
                    Repository = repository,
                    Comments = new List<CommentData>(),
                    Tags = new List<WorkItemTagData>(),
                };

                if (workitem.HaveLocalId())
                {
                    workitem.GitHubLocalId = info.LocalId;
                }

                // Add the work item to the context.
                request.Context.WorkItems.Add(workitem);
            }
            else
            {
                // Update the work item.
                workitem.Title = info.Title;
                workitem.Preamble = GeneratePreamble(info);
                workitem.Body = info.Body;
                workitem.UpdatedAt = info.UpdatedAt;
                workitem.IsOpen = info.State == WorkItemState.Open;
                workitem.IsClosed = info.State == WorkItemState.Closed;
                workitem.IsReopened = info.State == WorkItemState.Reopened;
                workitem.IsDraft = info.Draft;
                workitem.Merged = info.Merged;
                workitem.MergedAt = info.MergedAt;
                workitem.Locked = info.Locked;

                // Make sure changes are tracked.
                request.Context.WorkItems.Update(workitem);
            }

            // Is this a PR that was merged?
            if (workitem.MergedBy == null && !string.IsNullOrWhiteSpace(info.MergedBy))
            {
                var mergedByResult = await _mediator.Send(new GetGitHubUser.Request(request.Gateway, request.Context, info.MergedBy, true), cancellationToken);
                if (mergedByResult.Faulted)
                {
                    return mergedByResult.ForCaller(nameof(UpdateGitHubWorkItem))
                        .Track(_telemetry, "Could not retrieve user who merged pull request.")
                        .Log(_log, "Could retrieve get user {GitHubMergedByUser} who merged pull request.", info.MergedBy)
                        .GetResult().Convert<WorkItemData>();
                }

                workitem.MergedBy = mergedByResult.Unwrap();
            }

            // Update milestone.
            var milestoneResult = await _mediator.UpdateGitHubWorkItemMilestone(request.Gateway, request.Context, workitem, info.Milestone);
            if (milestoneResult.Faulted)
            {
                return milestoneResult.ForCaller(nameof(UpdateGitHubWorkItem))
                    .Track(_telemetry, "Could not update milestone information for work item.")
                    .Log(_log, "Could not update milestone information for work item {GitHubWorkItemId}.", workitem.GitHubId)
                    .GetResult().Convert<WorkItemData>();
            }

            // Update comments.
            if (workitem.SupportComments())
            {
                if (workitem.IsCommit())
                {
                    var commentResult = await _mediator.Send(new UpdateGitHubCommitComments.Request(request.Gateway, request.Context, workitem), cancellationToken);
                    if (commentResult.Faulted)
                    {
                        return commentResult.ForCaller(nameof(UpdateGitHubWorkItem))
                            .Track(_telemetry, "Could not update comments for commit.")
                            .Log(_log, "Could not update comments for commit {GitHubCommitId}.", workitem.GitHubId)
                            .GetResult().Convert<WorkItemData>();
                    }
                }
                else
                {
                    var commentResult = await _mediator.UpdateGitHubWorkItemComments(request.Gateway, request.Context, workitem);
                    if (commentResult.Faulted)
                    {
                        return commentResult.ForCaller(nameof(UpdateGitHubWorkItem))
                            .Track(_telemetry, "Could not update comments for work item.")
                            .Log(_log, "Could not update comments for work item {GitHubWorkItemId}.", workitem.GitHubId)
                            .GetResult().Convert<WorkItemData>();
                    }
                }
            }

            // Update labels.
            if (info.Labels != null)
            {
                var labelResult = await _mediator.UpdateGitHubWorkItemLabels(request.Gateway, request.Context, workitem, info.Labels);
                if (labelResult.Faulted)
                {
                    return labelResult.ForCaller(nameof(UpdateGitHubWorkItem))
                        .Track(_telemetry, "Could not update labels for pull request.")
                        .Log(_log, "Could not update labels for pull request {GitHubPullRequestId}.", workitem.GitHubId)
                        .GetResult().Convert<WorkItemData>();
                }
            }

            // Update assignees.
            if (workitem.SupportAssignees())
            {
                var assigneeResult = await _mediator.UpdateGitHubWorkItemAssignees(request.Gateway, request.Context, workitem, info.Assignees);
                if (assigneeResult.Faulted)
                {
                    return assigneeResult.ForCaller(nameof(UpdateGitHubWorkItem))
                        .Track(_telemetry, "Could not update assignees for pull request.")
                        .Log(_log, "Could not update assignees for pull request {GitHubPullRequestId}.", workitem.GitHubId)
                        .GetResult().Convert<WorkItemData>();
                }
            }

            // Update reviews.
            if (workitem.IsPullRequest.GetSafeValue())
            {
                var updateReviewResults = await _mediator.Send(new UpdateGitHubPullRequestReviews.Request(request.Gateway, request.Context, workitem), cancellationToken);
                if (updateReviewResults.Faulted)
                {
                    return updateReviewResults.ForCaller(nameof(UpdateGitHubWorkItem))
                        .Track(_telemetry, "Could not update reviews for pull request.")
                        .Log(_log, "Could not update reviews for pull request {GitHubPullRequestId}.", workitem.GitHubId)
                        .GetResult().Convert<WorkItemData>();
                }
            }

            return GitHubResult.Ok(workitem);
        }

        private string GeneratePreamble(GitHubWorkItemInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.Body))
            {
                return null;
            }

            var preamble = Markdig.Markdown.ToPlainText(info.Body);
            preamble = WebUtility.HtmlDecode(preamble);
            preamble = preamble.Replace("\r\n", " ").Replace("\n", " ");

            if (preamble.Length > 255)
            {
                preamble = preamble.Substring(0, Math.Min(preamble.Length, 255)) + "...";
            }

            return preamble;
        }
    }
}

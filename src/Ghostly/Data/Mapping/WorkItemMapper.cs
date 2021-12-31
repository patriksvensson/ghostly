using System;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;

namespace Ghostly.Data.Mapping
{
    public static class WorkItemMapper
    {
        public static WorkItem Map(WorkItemData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Discriminator == Discriminator.GitHub)
            {
                return new GitHubWorkItem
                {
                    Id = data.Id,
                    Kind = GetKind(data),
                    State = GetState(data),
                    Tags = TagMapper.Map(data.Tags),
                    GitHubId = data.GitHubId.Value,
                    Url = new Uri(data.Url, UriKind.RelativeOrAbsolute),
                    Title = data.Title,
                    Preamble = data.Preamble,
                    Body = data.Body,
                    Merged = data.Merged,
                    Locked = data.Locked != null && data.Locked.Value,
                    CreatedAt = data.CreatedAt.Value.EnsureUniversalTime(),
                    UpdatedAt = data.UpdatedAt?.EnsureUniversalTime(),
                    MergedAt = data.MergedAt?.EnsureUniversalTime(),
                    IsDraft = data.IsDraft,
                    CommitSha = data.CommitSha,
                    Author = UserMapper.Map(data.Author),
                    MergedBy = UserMapper.Map(data.MergedBy) as GitHubUser,
                    Milestone = MilestoneMapper.Map(data.Milestone),
                    Repository = RepositoryMapper.Map(data.Repository),
                    Comments = CommentMapper.Map(data.Comments),
                    Reviews = ReviewMapper.Map(data.Reviews),
                };
            }

            throw new InvalidOperationException("Do not know how to map work item.");
        }

        public static GitHubWorkItemKind GetKind(WorkItemData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.IsPullRequest.Value)
            {
                return GitHubWorkItemKind.PullRequest;
            }

            if (data.IsIssue.Value)
            {
                return GitHubWorkItemKind.Issue;
            }

            if (data.IsRelease.Value)
            {
                return GitHubWorkItemKind.Release;
            }

            if (data.IsVulnerability.Value)
            {
                return GitHubWorkItemKind.Vulnerability;
            }

            if (data.IsCommit.Value)
            {
                return GitHubWorkItemKind.Commit;
            }

            if (data.IsDiscussion.Value)
            {
                return GitHubWorkItemKind.Discussion;
            }

            return GitHubWorkItemKind.Unknown;
        }

        public static WorkItemState GetState(WorkItemData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.IsOpen)
            {
                return WorkItemState.Open;
            }

            if (data.IsClosed)
            {
                return WorkItemState.Closed;
            }

            if (data.IsReopened)
            {
                return WorkItemState.Reopened;
            }

            return WorkItemState.Unknown;
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;
using Ghostly.GitHub.Actions;
using Ghostly.GitHub.Octokit;
using MediatR;

namespace Ghostly.GitHub
{
    internal static class MediatorExtensions
    {
        public static async Task<GitHubResult<WorkItemData>> GetGitHubWorkItem(
                    this IMediator mediator, GhostlyContext context,
                    GitHubWorkItemKind kind, long githubId, long githubRepositoryId)
        {
            return await mediator.Send(new GetGitHubWorkItem.Request(context, kind, githubId, githubRepositoryId));
        }

        public static async Task<GitHubResult<GitHubWorkItemInfo>> GetGitHubWorkItemInformation(
            this IMediator mediator, IGitHubGateway gateway, GitHubNotificationItem info)
        {
            return await mediator.Send(new GetGitHubWorkItemInformation.Request(gateway, info));
        }

        public static async Task<GitHubResult<RepositoryData>> GetGitHubRepository(
            this IMediator mediator, GhostlyContext context, long githubId)
        {
            return await mediator.Send(new GetGitHubRepository.Request(context, githubId));
        }

        public static async Task<GitHubResult<UpdateGitHubNotification.Response>> UpdateGitHubNotification(
                    this IMediator mediator, IGitHubGateway gateway,
                    Account account, GitHubNotificationItem info,
                    bool force = false)
        {
            return await mediator.Send(new UpdateGitHubNotification.Request(gateway, account, info, force));
        }

        public static async Task<GitHubResult<WorkItemData>> UpdateGitHubWorkItem(
            this IMediator mediator, IGitHubGateway gateway,
            GhostlyContext context, GitHubNotificationItem info)
        {
            return await mediator.Send(new UpdateGitHubWorkItem.Request(gateway, context, info));
        }

        public static async Task<GitHubResult> UpdateGitHubWorkItemComments(
            this IMediator mediator, IGitHubGateway gateway,
            GhostlyContext context, WorkItemData workitem)
        {
            return await mediator.Send(new UpdateGitHubWorkItemComments.Request(gateway, context, workitem));
        }

        public static async Task<GitHubResult> UpdateGitHubWorkItemLabels(
            this IMediator mediator, IGitHubGateway gateway,
            GhostlyContext context, WorkItemData workitem,
            IEnumerable<GitHubLabelInfo> labels)
        {
            return await mediator.Send(new UpdateGitHubWorkItemLabels.Request(gateway, context, workitem, labels));
        }

        public static async Task<GitHubResult> UpdateGitHubWorkItemAssignees(
            this IMediator mediator, IGitHubGateway gateway,
            GhostlyContext context, WorkItemData workitem,
            IEnumerable<string> assignees)
        {
            return await mediator.Send(new UpdateGitHubWorkItemAssignees.Request(gateway, context, workitem, assignees));
        }

        public static async Task<GitHubResult> UpdateGitHubWorkItemMilestone(
            this IMediator mediator, IGitHubGateway gateway,
            GhostlyContext context, WorkItemData workitem,
            GitHubMilestoneInfo milestone)
        {
            return await mediator.Send(new UpdateGitHubWorkItemMilestone.Request(gateway, context, workitem, milestone));
        }
    }
}

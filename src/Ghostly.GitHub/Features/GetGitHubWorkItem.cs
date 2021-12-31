using System;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Domain.GitHub;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.GitHub.Actions
{
    internal sealed class GetGitHubWorkItem : GitHubRequestHandler<GetGitHubWorkItem.Request, WorkItemData>
    {
        public sealed class Request : IRequest<GitHubResult<WorkItemData>>
        {
            public GhostlyContext Context { get; }
            public GitHubWorkItemKind Kind { get; }
            public long GitHubId { get; }
            public long GitHubRepositoryId { get; }

            public Request(GhostlyContext context, GitHubWorkItemKind kind, long githubId, long githubRepositoryId)
            {
                Context = context ?? throw new ArgumentNullException(nameof(context));
                Kind = kind;
                GitHubId = githubId;
                GitHubRepositoryId = githubRepositoryId;
            }
        }

        protected override async Task<GitHubResult<WorkItemData>> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            if (request.Kind == GitHubWorkItemKind.Unknown)
            {
                throw new InvalidOperationException("Unknown notification kind.");
            }

            var isPullRequest = request.Kind == GitHubWorkItemKind.PullRequest;
            var isIssue = request.Kind == GitHubWorkItemKind.Issue;
            var isRelease = request.Kind == GitHubWorkItemKind.Release;
            var isVulnerability = request.Kind == GitHubWorkItemKind.Vulnerability;
            var isDiscussion = request.Kind == GitHubWorkItemKind.Discussion;

            var result = await request.Context.WorkItems
                .Include(n => n.Repository)
                .Include(n => n.Author)
                .Include(n => n.MergedBy)
                .Include(n => n.Tags)
                    .ThenInclude(t => t.Tag)
                .Include(n => n.Reviews)
                    .ThenInclude(r => r.Author)
                .Include(n => n.Reviews)
                    .ThenInclude(r => r.Comments)
                    .ThenInclude(r => r.Author)
                .Include(n => n.Comments)
                    .ThenInclude(n => n.Author)
                .FirstOrDefaultAsync(x => x.Discriminator == Discriminator.GitHub
                    && x.IsPullRequest == isPullRequest
                    && x.IsIssue == isIssue
                    && x.IsRelease == isRelease
                    && x.IsVulnerability == isVulnerability
                    && x.IsDiscussion == isDiscussion
                    && x.GitHubLocalId == request.GitHubId
                    && x.Repository.GitHubId == request.GitHubRepositoryId,
                    cancellationToken);

            return GitHubResult.Ok(result);
        }
    }
}

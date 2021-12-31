using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.GitHub.Octokit;
using MediatR;

namespace Ghostly.GitHub.Actions
{
    internal sealed class UpdateGitHubWorkItemComments : GitHubRequestHandler<UpdateGitHubWorkItemComments.Request>
    {
        private readonly IMediator _mediator;
        private readonly ITelemetry _telemetry;
        private readonly IGhostlyLog _log;

        public UpdateGitHubWorkItemComments(
            IMediator mediator,
            ITelemetry telemetry,
            IGhostlyLog log)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public sealed class Request : IRequest<GitHubResult>
        {
            public IGitHubGateway Gateway { get; }
            public GhostlyContext Context { get; }
            public WorkItemData Workitem { get; }

            public Request(IGitHubGateway gateway, GhostlyContext context, WorkItemData workitem)
            {
                Gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
                Context = context ?? throw new ArgumentNullException(nameof(context));
                Workitem = workitem ?? throw new ArgumentNullException(nameof(workitem));
            }
        }

        protected override async Task<GitHubResult> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            var workitem = request.Workitem;
            var owner = workitem.Repository.Owner;
            var repo = workitem.Repository.Name;

            if (!workitem.SupportComments())
            {
                return GitHubResult.Ok();
            }

            // Get all comments for the work item.
            var commentsResult = await request.Gateway.GetIssueComments(owner, repo, workitem.GitHubLocalId.Value);
            if (commentsResult.Faulted)
            {
                return commentsResult.ForCaller(nameof(UpdateGitHubWorkItemComments))
                    .Track(_telemetry, "Could not retrieve comments for work item.")
                    .Log(_log, "Could not retrieve comments for work item #{GitHubWorkItemId}", workitem.GitHubLocalId)
                    .GetResult();
            }

            var comments = commentsResult.Unwrap();
            foreach (var comment in comments)
            {
                var existing = workitem.Comments.FirstOrDefault(x => x.GitHubId == comment.Id);
                if (existing == null)
                {
                    var authorResult = await _mediator.Send(new GetGitHubUser.Request(request.Gateway, request.Context, comment.User.Login, true), cancellationToken);
                    if (authorResult.Faulted)
                    {
                        return authorResult.ForCaller(nameof(UpdateGitHubWorkItemComments))
                            .Track(_telemetry, "Could not retrieve user for work item comment.")
                            .Log(_log, "Could not retrieve user @{GitHubLogin} for work item comment #{GitHubCommentId}", comment.User.Login, comment.Id)
                            .GetResult();
                    }

                    workitem.Comments.Add(new CommentData
                    {
                        Discriminator = Discriminator.GitHub,
                        GitHubId = comment.Id,
                        Url = comment.HtmlUrl,
                        Body = comment.Body,
                        Author = authorResult.Unwrap(),
                        CreatedAt = comment.CreatedAt.UtcDateTime,
                        UpdatedAt = comment?.UpdatedAt?.UtcDateTime,
                    });
                }
                else
                {
                    existing.Body = comment.Body;
                    existing.UpdatedAt = comment?.UpdatedAt?.UtcDateTime;
                }
            }

            // Remove removed comments.
            workitem.Comments.RemoveAll(x => !comments.Any(y => y.Id == x.GitHubId));

            return GitHubResult.Ok();
        }
    }
}

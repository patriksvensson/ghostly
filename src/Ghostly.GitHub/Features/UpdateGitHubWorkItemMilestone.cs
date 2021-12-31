using System;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.GitHub.Octokit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.GitHub.Actions
{
    internal sealed class UpdateGitHubWorkItemMilestone : GitHubRequestHandler<UpdateGitHubWorkItemMilestone.Request>
    {
        public sealed class Request : IRequest<GitHubResult>
        {
            public IGitHubGateway Gateway { get; }
            public GhostlyContext Context { get; }
            public WorkItemData Workitem { get; }
            public GitHubMilestoneInfo Milestone { get; }

            public Request(IGitHubGateway gateway, GhostlyContext context, WorkItemData workitem, GitHubMilestoneInfo milestone)
            {
                Gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
                Context = context ?? throw new ArgumentNullException(nameof(context));
                Workitem = workitem ?? throw new ArgumentNullException(nameof(workitem));
                Milestone = milestone;
            }
        }

        protected override async Task<GitHubResult> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            var context = request.Context;
            var workitem = request.Workitem;

            if (request.Milestone == null)
            {
                if (workitem.Milestone != null)
                {
                    workitem.Milestone = null;
                }
            }
            else
            {
                var milestone = await request.Context.Set<MilestoneData>().SingleOrDefaultAsync(m => m.GitHubId == request.Milestone.Id, cancellationToken);
                if (milestone == null)
                {
                    milestone = new MilestoneData();
                    milestone.GitHubId = request.Milestone.Id;
                    milestone.Name = request.Milestone.Name;
                    context.Add(milestone);
                }

                workitem.Milestone = milestone;
            }

            return GitHubResult.Ok();
        }
    }
}

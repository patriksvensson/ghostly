using System;
using System.Collections.Generic;
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
    internal sealed class UpdateGitHubWorkItemAssignees : GitHubRequestHandler<UpdateGitHubWorkItemAssignees.Request>
    {
        private readonly IMediator _mediator;
        private readonly ITelemetry _telemetry;
        private readonly IGhostlyLog _log;

        public UpdateGitHubWorkItemAssignees(
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
            public IReadOnlyList<string> Assignees { get; }

            public Request(IGitHubGateway gateway, GhostlyContext context, WorkItemData workitem, IEnumerable<string> assignees)
            {
                Gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
                Context = context ?? throw new ArgumentNullException(nameof(context));
                Workitem = workitem ?? throw new ArgumentNullException(nameof(workitem));
                Assignees = assignees?.ToReadOnlyList() ?? new List<string>();
            }
        }

        protected override async Task<GitHubResult> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            if (!request.Workitem.SupportAssignees())
            {
                return GitHubResult.Ok();
            }

            foreach (var assignee in request.Assignees)
            {
                var assigneeData = request.Workitem.Assignees?.SingleOrDefault(x => x.User.Discriminator == Discriminator.GitHub && x.User.Login == assignee)?.User;
                if (assigneeData == null)
                {
                    _log.Verbose("Adding assignee: {LabelName}", assignee);
                    await AddAssignee(request, assignee);
                }
            }

            return GitHubResult.Ok();
        }

        private async Task<GitHubResult> AddAssignee(Request request, string assignee)
        {
            var assigneeResult = await _mediator.Send(new GetGitHubUser.Request(request.Gateway, request.Context, assignee, true));
            if (assigneeResult.Faulted)
            {
                return assigneeResult.ForCaller(nameof(UpdateGitHubWorkItemAssignees))
                    .Track(_telemetry, "Could not retrieve pull request assignee.")
                    .Log(_log, "Could not retrieve pull request assignee {GitHubAssignee}.", assignee)
                    .GetResult();
            }

            if (request.Workitem.Assignees == null)
            {
                request.Workitem.Assignees = new List<AssigneeData>();
            }

            // Add the assignee to the work item.
            request.Workitem.Assignees.Add(new AssigneeData
            {
                Discriminator = Discriminator.GitHub,
                User = assigneeResult.Unwrap(),
            });

            return GitHubResult.Ok();
        }
    }
}

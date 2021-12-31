using System.Threading;
using System.Threading.Tasks;
using Ghostly.GitHub.Octokit;
using MediatR;
using OctokitNotification = Octokit.Notification;

namespace Ghostly.GitHub.Features
{
    internal sealed class GetRemoteGitHubNotification : GitHubRequestHandler<GetRemoteGitHubNotification.Request, OctokitNotification>
    {
        public sealed class Request : IRequest<GitHubResult<OctokitNotification>>
        {
            public IGitHubGateway Gateway { get; }
            public long GithubId { get; }

            public Request(IGitHubGateway gateway, long githubId)
            {
                Gateway = gateway;
                GithubId = githubId;
            }
        }

        protected override Task<GitHubResult<OctokitNotification>> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            return request.Gateway.Execute(c => c.Activity.Notifications.Get(request.GithubId));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.GitHub.Octokit;
using MediatR;
using OctokitNotification = Octokit.Notification;

namespace Ghostly.GitHub.Actions
{
    internal sealed class GetGitHubNotifications : GitHubRequestHandler<GetGitHubNotifications.Request, IReadOnlyList<OctokitNotification>>
    {
        public sealed class Request : IRequest<GitHubResult<IReadOnlyList<OctokitNotification>>>
        {
            public IGitHubGateway Gateway { get; }
            public DateTime? Timestamp { get; }
            public bool IncludePrivate { get; }

            public Request(IGitHubGateway gateway, DateTime? timestamp, bool includePrivate)
            {
                Gateway = gateway;
                Timestamp = timestamp;
                IncludePrivate = includePrivate;
            }
        }

        protected override async Task<GitHubResult<IReadOnlyList<OctokitNotification>>> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            // First, get all unread items.
            var unreadResult = await request.Gateway.GetUnreadNotifications();
            if (unreadResult.Faulted)
            {
                return unreadResult;
            }

            // Now get everything from the last 14 days.
            var allResult = await request.Gateway.GetAllNotifications(request.Timestamp);
            if (allResult.Faulted)
            {
                return allResult;
            }

            var unread = unreadResult.Unwrap();
            var all = allResult.Unwrap();

            // Combine the two results.
            var unique = new HashSet<OctokitNotification>(new OctokitNotificationComparer());
            unique.AddRange(Filter(unread, request.IncludePrivate));
            unique.AddRange(Filter(all, request.IncludePrivate));

            // Return the result.
            return GitHubResult.Ok<IReadOnlyList<OctokitNotification>>(new List<OctokitNotification>(unique));
        }

        private IEnumerable<OctokitNotification> Filter(IEnumerable<OctokitNotification> source, bool includePrivate)
        {
            if (source != null && !includePrivate)
            {
                return source.Where(n => !(n.Repository?.Private ?? false));
            }

            return source;
        }
    }
}

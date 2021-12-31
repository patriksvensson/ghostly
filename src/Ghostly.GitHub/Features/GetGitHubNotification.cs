using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Data;
using Ghostly.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.GitHub.Actions
{
    internal sealed class GetGitHubNotification : GitHubRequestHandler<GetGitHubNotification.Request, NotificationData>
    {
        public sealed class Request : IRequest<GitHubResult<NotificationData>>
        {
            public GhostlyContext Context { get; }
            public long GitHubId { get; }

            public Request(GhostlyContext context, long githubId)
            {
                Context = context;
                GitHubId = githubId;
            }
        }

        protected override async Task<GitHubResult<NotificationData>> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            // Check if we can find the notification locally.
            var result = QueryLocal(request);
            if (result == null)
            {
                // Query the database.
                // TODO: Use Notification query method here?
                result = await request.Context.Notifications
                    .Include(n => n.Category)
                    .Include(n => n.WorkItem)
                    .ThenInclude(n => n.Repository)
                    .FirstOrDefaultAsync(x => x.Discriminator == Discriminator.GitHub && x.GitHubId == request.GitHubId, cancellationToken);
            }

            return GitHubResult.Ok(result);
        }

        private static NotificationData QueryLocal(Request request)
        {
            return request.Context.Notifications.Local
                .Where(x => x.Discriminator == Discriminator.GitHub && x.GitHubId == request.GitHubId)
                .FirstOrDefault();
        }
    }
}

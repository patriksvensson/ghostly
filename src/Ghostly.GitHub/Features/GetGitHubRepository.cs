using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Data;
using Ghostly.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.GitHub.Actions
{
    internal sealed class GetGitHubRepository : GitHubRequestHandler<GetGitHubRepository.Request, RepositoryData>
    {
        public sealed class Request : IRequest<GitHubResult<RepositoryData>>
        {
            public GhostlyContext Context { get; }
            public long GitHubId { get; }

            public Request(GhostlyContext context, long gitHubId)
            {
                Context = context;
                GitHubId = gitHubId;
            }
        }

        protected override async Task<GitHubResult<RepositoryData>> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            var result = QueryLocal(request);
            if (result == null)
            {
                result = await request.Context.Repositories.FirstOrDefaultAsync(x => x.Discriminator == Discriminator.GitHub
                        && x.GitHubId == request.GitHubId, cancellationToken);
            }

            return GitHubResult.Ok(result);
        }

        private RepositoryData QueryLocal(Request request)
        {
            return request.Context.Repositories.Local.FirstOrDefault(
                x => x.Discriminator == Discriminator.GitHub
                    && x.GitHubId == request.GitHubId);
        }
    }
}

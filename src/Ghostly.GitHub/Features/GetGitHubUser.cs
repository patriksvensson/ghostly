using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.GitHub.Octokit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.GitHub.Actions
{
    internal sealed class GetGitHubUser : GitHubRequestHandler<GetGitHubUser.Request, UserData>
    {
        private readonly IMediator _mediator;

        public sealed class Request : IRequest<GitHubResult<UserData>>
        {
            public IGitHubGateway Gateway { get; }
            public GhostlyContext Context { get; }
            public string Login { get; }
            public bool IncludeLocal { get; }

            public Request(IGitHubGateway gateway, GhostlyContext context, string login, bool includeLocal = false)
            {
                Gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
                Context = context ?? throw new ArgumentNullException(nameof(context));
                Login = login ?? throw new ArgumentNullException(nameof(login));
                IncludeLocal = includeLocal;
            }
        }

        public GetGitHubUser(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override async Task<GitHubResult<UserData>> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            var database = await GetFromDatabase(request, cancellationToken);
            if (database.Faulted)
            {
                return database;
            }

            var user = database.Unwrap();
            if (user == null)
            {
                var github = await GetFromGitHub(request, cancellationToken);
                if (github.Faulted)
                {
                    return github;
                }

                user = github.Unwrap();
            }

            if (user != null)
            {
                // Download the avatar
                var avatarUrl = new Uri(user.AvatarUrl);
                await _mediator.Send(new DownloadGitHubAvatar.Request(avatarUrl, user.AvatarHash, deferred: true), cancellationToken);

                if (user.Id == 0)
                {
                    // Add the user to the context.
                    request.Context.Users.Add(user);
                }
            }

            return GitHubResult.Ok(user);
        }

        private async Task<GitHubResult<UserData>> GetFromDatabase(Request request, CancellationToken cancellationToken)
        {
            if (request.IncludeLocal)
            {
                var localUser = request.Context.Users.Local
                    .OfType<UserData>()
                    .FirstOrDefault(l => l.Discriminator == Discriminator.GitHub && l.Login == request.Login);

                if (localUser != null)
                {
                    return GitHubResult.Ok(localUser);
                }
            }

            return GitHubResult.Ok(await request.Context.Users.FirstOrDefaultAsync(x => x.Discriminator == Discriminator.GitHub
                    && x.Login == request.Login, cancellationToken));
        }

        private async Task<GitHubResult<UserData>> GetFromGitHub(Request request, CancellationToken cancellationToken)
        {
            var userResponse = await request.Gateway.GetUser(request.Login);
            if (userResponse.Faulted)
            {
                return userResponse.Convert<UserData>();
            }

            var user = userResponse.Unwrap();
            var hash = FilenameHasher.Calculate(user.AvatarUrl)
                .ToString(CultureInfo.InvariantCulture);

            return GitHubResult.Ok(new UserData
            {
                Discriminator = Discriminator.GitHub,
                Login = user.Login,
                Name = user.Name,
                GitHubId = user.Id,
                AvatarUrl = user.AvatarUrl,
                AvatarHash = hash,
            });
        }
    }
}

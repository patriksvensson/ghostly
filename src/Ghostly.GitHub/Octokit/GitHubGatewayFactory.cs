using System.Collections.Concurrent;
using Octokit;

namespace Ghostly.GitHub.Octokit
{
    internal interface IGitHubGatewayFactory
    {
        IGitHubGateway Create(string token);
    }

    internal sealed class GitHubGatewayFactory : IGitHubGatewayFactory
    {
        private readonly GitHubGateway.Factory _factory;
        private readonly ConcurrentDictionary<string, IGitHubGateway> _cache;

        public GitHubGatewayFactory(GitHubGateway.Factory factory)
        {
            _factory = factory;
            _cache = new ConcurrentDictionary<string, IGitHubGateway>();
        }

        public IGitHubGateway Create(string token)
        {
            if (_cache.TryGetValue(token, out var gateway))
            {
                return gateway;
            }

            gateway = _factory(new GitHubClient(new ProductHeaderValue("Ghostly"))
            {
                Credentials = new Credentials(token),
            });

            _cache.AddOrUpdate(token, gateway, (t, g) => gateway);

            return gateway;
        }
    }
}

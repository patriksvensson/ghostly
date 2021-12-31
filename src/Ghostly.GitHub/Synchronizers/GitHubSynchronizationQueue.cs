using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Features.Synchronization;

namespace Ghostly.GitHub.Synchronizers
{
    [DependentOn(typeof(DatabaseInitializer))]
    internal sealed class GitHubSynchronizationQueue : SynchronizationQueue<GitHubNotificationItem>, IInitializable
    {
        protected override Discriminator Discriminator => Discriminator.GitHub;

        public GitHubSynchronizationQueue(
            IGhostlyContextFactory factory,
            IGhostlyLog log)
            : base(factory, log)
        {
        }

        protected override bool RemoveItemFromCache(GitHubNotificationItem item)
        {
            return !item.Unread;
        }

        public Task<bool> Initialize(bool background)
        {
            Initialize();
            return Task.FromResult(true);
        }
    }
}

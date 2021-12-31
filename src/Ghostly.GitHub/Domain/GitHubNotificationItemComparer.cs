using System.Collections.Generic;

namespace Ghostly.GitHub
{
    internal sealed class GitHubNotificationItemComparer : EqualityComparer<GitHubNotificationItem>
    {
        public override bool Equals(GitHubNotificationItem x, GitHubNotificationItem y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Id == y.Id;
        }

        public override int GetHashCode(GitHubNotificationItem obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}

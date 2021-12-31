using System.Collections.Generic;
using OctokitNotification = Octokit.Notification;

namespace Ghostly.GitHub.Octokit
{
    internal sealed class OctokitNotificationComparer : IEqualityComparer<OctokitNotification>
    {
        public bool Equals(OctokitNotification x, OctokitNotification y)
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

        public int GetHashCode(OctokitNotification obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}

using System.Collections.Generic;

namespace Ghostly.Domain.Messages
{
    public sealed class NotificationStarred
    {
        public IReadOnlyList<Notification> Notifications { get; set; }
    }
}

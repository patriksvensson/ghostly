using System.Collections.Generic;

namespace Ghostly.Domain.Messages
{
    public sealed class NotificationMuted
    {
        public IReadOnlyList<Notification> Notifications { get; set; }
    }
}

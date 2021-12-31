using System.Collections.Generic;

namespace Ghostly.Domain.Messages
{
    public sealed class NotificationRead
    {
        public IReadOnlyList<Notification> Notifications { get; set; }
    }
}

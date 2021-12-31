using System.Collections.Generic;

namespace Ghostly.Domain.Messages
{
    public sealed class NotificationUnmuted
    {
        public IReadOnlyList<Notification> Notifications { get; set; }
    }
}

using System.Collections.Generic;

namespace Ghostly.Domain.Messages
{
    public sealed class NotificationUnstarred
    {
        public IReadOnlyList<Notification> Notifications { get; set; }
    }
}

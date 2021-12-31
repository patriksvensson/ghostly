using System.Collections.Generic;

namespace Ghostly.Domain.Messages
{
    public sealed class NotificationMoved
    {
        public Category Category { get; set; }
        public IReadOnlyList<Notification> Notifications { get; set; }
    }
}

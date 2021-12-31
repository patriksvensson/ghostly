using System.Collections.Generic;

namespace Ghostly.Domain.Messages
{
    public sealed class NewNotifications
    {
        public List<Notification> Notifications { get; }

        public NewNotifications(IEnumerable<Notification> notifications)
        {
            Notifications = notifications as List<Notification> ?? new List<Notification>(notifications);
        }
    }
}

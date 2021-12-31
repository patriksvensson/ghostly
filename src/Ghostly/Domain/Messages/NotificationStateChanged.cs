using System.Collections.Generic;

namespace Ghostly.Domain.Messages
{
    public enum UpdateNotificationState
    {
        Star,
        Unstar,
        Mute,
        Unmute,
    }

    public sealed class NotificationStateChanged
    {
        public IReadOnlyList<int> Notifications { get; set; }
        public UpdateNotificationState State { get; set; }
    }
}

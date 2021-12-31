using System.Collections.Generic;

namespace Ghostly.Domain.Messages
{
    public sealed class UnreadCountChanged
    {
        public int TotalUnread { get; set; }
        public IReadOnlyDictionary<int, int> TotalCount { get; set; }
        public IReadOnlyDictionary<int, int> UnreadCount { get; set; }
    }
}

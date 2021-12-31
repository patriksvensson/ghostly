using System;

namespace Ghostly.Data.Models
{
    public sealed class NotificationData : EntityData
    {
        public Discriminator Discriminator { get; set; }

        // Generic
        public int AccountId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Unread { get; set; }
        public bool Muted { get; set; }
        public bool Starred { get; set; }
        public WorkItemData WorkItem { get; set; }
        public CategoryData Category { get; set; }

        // GitHub specific
        public long? GitHubId { get; set; }
        public string Reason { get; set; }
    }
}

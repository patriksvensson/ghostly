using System;

namespace Ghostly.Data.Models
{
    public sealed class ActivityData : EntityData
    {
        public DateTime Timestamp { get; set; }
        public ActivityCategory Category { get; set; }
        public ActivityKind Kind { get; set; }
        public ActivityConstraint Constraint { get; set; }
        public string Payload { get; set; }
    }

    public enum ActivityCategory
    {
        None = 0,
        Synchronization = 1,
        Continuous = 2,
    }

    public enum ActivityConstraint
    {
        None = 0,
        RequiresInternetConnection = 1,
    }

    public enum ActivityKind
    {
        Unknown = 0,
        MarkAsRead = 1,
        DownloadFile = 2,
    }
}

using System;

namespace Ghostly.Domain
{
    public sealed class ReviewComment : Entity
    {
        public long GitHubId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string OriginalCommitId { get; set; }
        public string CommitId { get; set; }
        public string Path { get; set; }
        public int? Position { get; set; }
        public int? OriginalPosition { get; set; }
        public string Diff { get; set; }
        public Uri Url { get; set; }
        public string Body { get; set; }
        public long? InReplyToId { get; set; }
        public User Author { get; set; }
    }
}

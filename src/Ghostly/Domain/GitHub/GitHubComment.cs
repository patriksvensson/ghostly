using System;

namespace Ghostly.Domain.GitHub
{
    public sealed class GitHubComment : Comment
    {
        public int GitHubId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // We always want to show the time as for when it was created.
        public override DateTime Timestamp => CreatedAt;
    }
}

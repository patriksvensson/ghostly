using System;

namespace Ghostly.Domain.GitHub
{
    public sealed class GitHubWorkItem : WorkItem
    {
        public long GitHubId { get; set; }

        public GitHubRepository Repository { get; set; }
        public GitHubWorkItemKind Kind { get; set; }
        public bool Locked { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public GitHubUser MergedBy { get; set; }
        public DateTime? MergedAt { get; set; }

        // PR specific
        public bool? Merged { get; set; }
        public bool? IsDraft { get; set; }

        // Commit specific
        public string CommitSha { get; set; }

        // We always want to show the time as for when it was created.
        public override DateTime Timestamp => CreatedAt;

        // Map the origin.
        public override string Origin => $"{Repository.Owner}/{Repository.Name}";
    }
}

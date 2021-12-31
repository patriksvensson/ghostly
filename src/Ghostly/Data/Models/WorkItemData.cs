using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ghostly.Data.Models
{
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Data")]
    public sealed partial class WorkItemData : EntityData
    {
        public Discriminator Discriminator { get; set; }

        // Generic
        public bool IsOpen { get; set; }
        public bool IsClosed { get; set; }
        public bool IsReopened { get; set; }
        public List<WorkItemTagData> Tags { get; set; }

        // GitHub Specific
        public long? GitHubId { get; set; }
        public int? GitHubLocalId { get; set; }
        public bool? IsPullRequest { get; set; }
        public bool? IsIssue { get; set; }
        public bool? IsVulnerability { get; set; }
        public bool? IsDiscussion { get; set; }
        public bool? IsRelease { get; set; }
        public bool? IsCommit { get; set; }
        public bool? IsDraft { get; set; }
        public bool? Merged { get; set; }
        public bool? Locked { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Preamble { get; set; }
        public string Body { get; set; }
        public string CommitSha { get; set; }
        public UserData Author { get; set; }
        public UserData MergedBy { get; set; }
        public MilestoneData Milestone { get; set; }
        public RepositoryData Repository { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? MergedAt { get; set; }

        public List<CommentData> Comments { get; set; }
        public List<ReviewData> Reviews { get; set; }
        public List<AssigneeData> Assignees { get; set; }
    }
}

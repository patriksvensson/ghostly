using System;
using System.Collections.Generic;
using Ghostly.Data.Models;

namespace Ghostly.GitHub
{
    internal sealed class GitHubWorkItemInfo
    {
        public long GitHubId { get; set; }
        public long GitHubRepositoryId { get; set; }
        public int LocalId { get; set; }
        public int CommitId { get; set; }
        public string CommitSha { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Url { get; set; }
        public string User { get; set; }
        public bool Deleted { get; set; }
        public bool? Merged { get; set; }
        public string MergedBy { get; set; }
        public bool? Locked { get; set; }
        public bool? Draft { get; set; }
        public WorkItemState State { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? MergedAt { get; set; }
        public List<GitHubLabelInfo> Labels { get; set; }
        public List<string> Assignees { get; set; }
        public GitHubMilestoneInfo Milestone { get; set; }
    }
}

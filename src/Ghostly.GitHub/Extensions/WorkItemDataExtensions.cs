using Ghostly.Data.Models;

namespace Ghostly.GitHub
{
    internal static class WorkItemDataExtensions
    {
        public static bool HaveLocalId(this WorkItemData data)
        {
            return IsIssue(data) || IsPullRequest(data);
        }

        public static bool SupportComments(this WorkItemData data)
        {
            return IsIssue(data) || IsPullRequest(data) || IsCommit(data);
        }

        public static bool SupportAssignees(this WorkItemData data)
        {
            return IsIssue(data) || IsPullRequest(data);
        }

        public static bool IsCommit(this WorkItemData data)
        {
            return data?.IsCommit ?? false;
        }

        private static bool IsIssue(WorkItemData data)
        {
            return data?.IsIssue ?? false;
        }

        private static bool IsPullRequest(WorkItemData data)
        {
            return data?.IsPullRequest ?? false;
        }
    }
}

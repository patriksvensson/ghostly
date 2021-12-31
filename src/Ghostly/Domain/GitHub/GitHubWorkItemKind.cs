namespace Ghostly.Domain.GitHub
{
    public enum GitHubWorkItemKind
    {
        Unknown = 0,
        Issue,
        PullRequest,
        Release,
        Vulnerability,
        Commit,
        Discussion,
    }

    public static class GitHubWorkItemKindExtensions
    {
        public static bool RequiresAuthor(this GitHubWorkItemKind kind)
        {
            switch (kind)
            {
                case GitHubWorkItemKind.Vulnerability:
                case GitHubWorkItemKind.Discussion:
                    return false;
                default:
                    return true;
            }
        }
    }
}

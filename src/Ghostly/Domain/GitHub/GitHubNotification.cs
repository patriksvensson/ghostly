namespace Ghostly.Domain.GitHub
{
    public sealed class GitHubNotification : Notification
    {
        public long GitHubId { get; set; }
        public GitHubWorkItemKind Kind { get; set; }

        public override bool ShowExternalId => Kind != GitHubWorkItemKind.Vulnerability && Kind != GitHubWorkItemKind.Discussion;

        public override string Template
        {
            get
            {
                if (Kind == GitHubWorkItemKind.Vulnerability)
                {
                    return Constants.Templates.GitHubVulnerability;
                }

                if (Kind == GitHubWorkItemKind.Discussion)
                {
                    return Constants.Templates.GitHubDiscussion;
                }

                return base.Template;
            }
        }
    }
}

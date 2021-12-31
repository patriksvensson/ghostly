namespace Ghostly.Domain.GitHub
{
    public sealed class GitHubLabel : Tag
    {
        public long GitHubId { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
    }
}

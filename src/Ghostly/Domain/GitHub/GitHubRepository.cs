namespace Ghostly.Domain.GitHub
{
    public sealed class GitHubRepository : Repository
    {
        public long GitHubId { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }
    }
}

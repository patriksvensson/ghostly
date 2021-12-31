namespace Ghostly.GitHub
{
    internal sealed class GitHubLabelInfo
    {
        public long Id { get; set; }
        public string Owner { get; set; }
        public string Repository { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
    }
}

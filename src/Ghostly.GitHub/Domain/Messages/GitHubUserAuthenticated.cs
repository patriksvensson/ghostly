namespace Ghostly.GitHub
{
    internal sealed class GitHubUserAuthenticated
    {
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public string AccessToken { get; set; }
        public string[] Scopes { get; set; }
    }
}

using Ghostly.Core.Pal;

namespace Ghostly.GitHub
{
    internal static class GitHubPasswordVaultExtensions
    {
#if DEBUG
        private const string GithubResource = "ghostly://github/dev/accesstoken";
#else
        private const string GithubResource = "ghostly://github/accesstoken";
#endif

        public static string GetGitHubAccessToken(this IPasswordVault vault, string username)
        {
            return vault.GetPassword(GithubResource, username);
        }

        public static void SaveGitHubAccessToken(this IPasswordVault vault, string username, string accessToken)
        {
            DeleteGitHubAccessToken(vault, username);
            vault.SetPassword(GithubResource, username, accessToken);
        }

        public static void DeleteGitHubAccessToken(this IPasswordVault vault, string username)
        {
            var password = vault.GetGitHubAccessToken(username);
            if (password != null)
            {
                vault.DeletePassword(GithubResource, username, password);
            }
        }
    }
}

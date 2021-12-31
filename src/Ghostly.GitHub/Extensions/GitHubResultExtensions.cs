using System;
using System.Diagnostics.CodeAnalysis;

namespace Ghostly.GitHub
{
    internal static class GitHubResultExtensions
    {
        public static GitHubResultTracker ForCaller(this GitHubResult result, string caller)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            return new GitHubResultTracker(result, caller);
        }

        public static GitHubResultTracker<T> ForCaller<T>(this GitHubResult<T> result, string caller)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            return new GitHubResultTracker<T>(result, caller);
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Extension method")]
        public static T WithResult<T>(this GitHubResultTracker tracker, T result)
        {
            return result;
        }
    }
}

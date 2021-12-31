using System;
using Ghostly.Core.Mvvm;

namespace Ghostly.GitHub.ViewModels
{
    public sealed class GitHubPreferencesViewModel : DialogScreen<GitHubPreferencesViewModel.Request, GitHubAuthenticationPreferences>
    {
        public bool PrivateRepositories { get; set; }
        public bool Gists { get; set; }

        public sealed class Request : IDialogRequest<GitHubAuthenticationPreferences>
        {
        }

        public override void Initialize(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            PrivateRepositories = true;
            Gists = false;
        }

        public override GitHubAuthenticationPreferences GetResult(bool ok)
        {
            if (ok)
            {
                return new GitHubAuthenticationPreferences
                {
                    PrivateRepositories = PrivateRepositories,
                    Gists = Gists,
                };
            }

            return null;
        }
    }
}

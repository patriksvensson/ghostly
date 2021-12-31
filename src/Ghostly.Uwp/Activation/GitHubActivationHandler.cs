using System;
using System.Threading.Tasks;
using System.Web;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.GitHub;
using Windows.ApplicationModel.Activation;

namespace Ghostly.Uwp.Activation
{
    public sealed class GitHubActivationHandler : ActivationHandler<IProtocolActivatedEventArgs>
    {
        private readonly IGitHubAuthenticator _authenticator;
        private readonly IGhostlyLog _log;

        public GitHubActivationHandler(IGitHubAuthenticator github, IGhostlyLog log)
        {
            _authenticator = github;
            _log = log;
        }

        public override bool CanHandle(IProtocolActivatedEventArgs args)
        {
            return args?.Uri?.Host?.Equals("github", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public override async Task Handle(IProtocolActivatedEventArgs args)
        {
            _log.Debug("[GitHubActivationHandler] BackgroundActivated={BackgroundActivated}", GhostlyState.IsBackgroundActivated);

            var path = args?.Uri.LocalPath ?? string.Empty;
            if (path.Equals("/callback", StringComparison.OrdinalIgnoreCase))
            {
                var queryString = HttpUtility.ParseQueryString(args.Uri.Query);
                var code = queryString.Get("code");
                if (code != null)
                {
                    await _authenticator.Authenticate(code);
                }
            }
        }
    }
}

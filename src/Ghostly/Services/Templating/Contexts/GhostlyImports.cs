using System;
using System.Collections.Generic;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Services.Templating.Objects;
using Scriban.Runtime;

namespace Ghostly.Services.Templating.Contexts
{
    public sealed class GhostlyImports : IScriptObjectImportable
    {
        private readonly INetworkHelper _network;
        private readonly ILocalizer _localizer;
        private readonly IClock _clock;

        public Notification Notification { get; set; }
        public WorkItem WorkItem { get; set; }
        public IReadOnlyList<ITemplateItem> Items { get; set; }
        public string Theme { get; set; }
        public bool ScrollToLastComment { get; set; }
        public bool ShowAvatars { get; set; }
        public bool PreferInternetAvatars { get; set; }
        public bool AllowMetered { get; set; }

        public GhostlyImports(
            INetworkHelper network,
            ILocalizer localizer,
            IClock clock)
        {
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public void RegisterMethods(ScriptObject model)
        {
            model.Import("avatar_url", new Func<User, string>(GetAvatarUrl));
            model.Import("to_html", new Func<string, string>(ToHtml));
            model.Import("localize", new Func<string, string>(Localize));
            model.Import("lowercase", new Func<string, string>(Lowercase));
            model.Import("humanize", new Func<DateTime, string>(Humanize));
        }

        public string ToHtml(string markdown)
        {
            return MarkdownImports.ConvertToHtml(markdown,
                () => _localizer.GetString("Template_NoDescriptionProvided"));
        }

        public string Localize(string key)
        {
            return _localizer[key];
        }

        public string Lowercase(string text)
        {
            return text?.ToLowerInvariant() ?? string.Empty;
        }

        public string Humanize(DateTime timestamp)
        {
            return timestamp.Humanize(_clock, _localizer).ToLowerInvariant();
        }

        public string GetAvatarUrl(User user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (_network.IsConnected && PreferInternetAvatars)
            {
                // Not on a metered connection, or on a meterered connction
                // but syncing data is OK?
                if (!_network.IsMetered || (_network.IsMetered && AllowMetered))
                {
                    return $"{user.AvatarUrl}?s=44";
                }
            }

            return $"Images/Avatars/{user.AvatarHash}.png?fallback=Images/Avatar.png";
        }
    }
}

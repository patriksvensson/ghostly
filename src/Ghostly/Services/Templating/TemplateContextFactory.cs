using System;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Services.Templating.Contexts;
using Scriban;
using Scriban.Runtime;

namespace Ghostly.Services.Templating
{
    public static class TemplateContextFactory
    {
        private static readonly MarkdownImports _markdown;

        static TemplateContextFactory()
        {
            _markdown = new MarkdownImports();
        }

        public static TemplateContext Create(
            INetworkHelper network,
            ILocalizer localizer,
            IClock clock,
            Action<GhostlyImports> action)
        {
            if (network is null)
            {
                throw new ArgumentNullException(nameof(network));
            }

            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            if (clock is null)
            {
                throw new ArgumentNullException(nameof(clock));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // Configure the Ghostly imports.
            var ghostly = new GhostlyImports(network, localizer, clock);
            action(ghostly);

            // Create the script object.
            var model = new ScriptObject();
            model.Import(_markdown);
            model.ImportCustom(ghostly);

            // Create the context.
            var context = new TemplateContext();
            context.PushGlobal(model);
            return context;
        }
    }
}

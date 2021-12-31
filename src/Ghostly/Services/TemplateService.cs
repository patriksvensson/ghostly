using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Services.Templating;
using Ghostly.Services.Templating.Objects;
using Scriban;

namespace Ghostly.Services
{
    public interface ITemplateService
    {
        Task<string> RenderNotification(Notification notification, WorkItem workitem);
    }

    [DependentOn(typeof(DatabaseInitializer))]
    [DependentOn(typeof(ILocalSettings))]
    [DependentOn(typeof(IThemeService))]
    public sealed class TemplateService : ITemplateService, IInitializable, IBackgroundInitializable
    {
        private readonly IResourceReader _reader;
        private readonly ILocalSettings _settings;
        private readonly IThemeService _theme;
        private readonly INetworkHelper _network;
        private readonly ILocalizer _localizer;
        private readonly IClock _clock;
        private readonly IGhostlyLog _log;
        private readonly TemplateItemBuilder _builder;
        private readonly ConcurrentDictionary<string, Template> _templates;

        private bool _scrollToLastComment;
        private bool _showAvatars;
        private bool _allowMetered;
        private bool _preferInternetAvatars;

        public TemplateService(
            IResourceReader reader,
            ILocalSettings settings,
            IThemeService theme,
            INetworkHelper network,
            IMessageService messenger,
            ILocalizer localizer,
            IClock clock,
            IGhostlyLog log)
        {
            if (messenger is null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }

            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _templates = new ConcurrentDictionary<string, Template>();
            _builder = new TemplateItemBuilder(localizer);

            messenger.Subscribe<SettingUpdated>(OnSettingUpdated);
        }

        public Task<bool> Initialize(bool background)
        {
            UpdateSettings();
            return Task.FromResult(true);
        }

        public async Task InitializeInBackground()
        {
            // Load all templates in the background.
            foreach (var template in Constants.Templates.GetAll())
            {
                await LoadTemplate(template);
            }
        }

        public async Task<string> RenderNotification(Notification notification, WorkItem workitem)
        {
            if (notification is null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            if (workitem is null)
            {
                throw new ArgumentNullException(nameof(workitem));
            }

            // Build the aggregated list of comments
            _log.Verbose("Extracting template items from notification {NotificationId}...", notification.Id);
            var items = _builder.Build(workitem);

            // Load and render the template
            _log.Verbose("Loading template {TemplatePath} for notification {NotificationId}...", notification.Template, notification.Id);
            var template = await LoadTemplate(notification.Template);

            // Render the HTML
            _log.Verbose("Rendering notification {NotificationId}...", notification.Id);
            var html = await template.RenderAsync(
                TemplateContextFactory.Create(_network, _localizer, _clock, inner =>
                {
                    inner.Theme = _theme.Canonical.GetCanonicalName();
                    inner.Notification = notification;
                    inner.WorkItem = workitem;
                    inner.Items = items;
                    inner.ScrollToLastComment = _scrollToLastComment;
                    inner.ShowAvatars = _showAvatars;
                    inner.AllowMetered = _allowMetered;
                    inner.PreferInternetAvatars = _preferInternetAvatars;
                }));

            // Return the rendererd HTML
            return html;
        }

        private async Task<Template> LoadTemplate(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("No template provided.");
            }

            if (!_templates.TryGetValue(path, out var template))
            {
                var stream = _reader.Read(path);
                if (stream == null)
                {
                    throw new InvalidOperationException("Could not read template.");
                }

                using (var reader = new StreamReader(stream))
                {
                    var source = await reader.ReadToEndAsync();
                    template = Template.Parse(source);
                }

                _templates.TryAdd(path, template);
            }

            return template;
        }

        private void UpdateSettings()
        {
            _scrollToLastComment = _settings.GetScrollToLastComment();
            _showAvatars = _settings.GetShowAvatars();
            _allowMetered = _settings.GetAllowMeteredConnection();
            _preferInternetAvatars = _settings.GetPreferInternetAvatars();
        }

        private void OnSettingUpdated(SettingUpdated message)
        {
            if (message.SettingName == Constants.Settings.ScrollToLastComment ||
                message.SettingName == Constants.Settings.ShowAvatars ||
                message.SettingName == Constants.Settings.AllowMeteredConnection ||
                message.SettingName == Constants.Settings.PreferInternetAvatars)
            {
                UpdateSettings();
            }
        }
    }
}

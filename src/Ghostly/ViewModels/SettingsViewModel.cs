using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Mvvm;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Services;

namespace Ghostly.ViewModels
{
    [DependentOn(typeof(DatabaseInitializer))]
    [DependentOn(typeof(ICultureService))]
    [DependentOn(typeof(IThemeService))]
    public sealed class SettingsViewModel : Screen, IInitializable
    {
        private readonly IPackageService _package;
        private readonly ILocalSettings _settings;
        private readonly IMessageService _messenger;
        private readonly IFolderLauncher _folderLauncher;
        private readonly ILogLevelSwitch _logLevelSwitch;
        private readonly IStartupHelper _startup;
        private readonly ICultureService _cultures;
        private readonly ILocalizer _localizer;
        private readonly IBadgeUpdater _badge;
        private readonly IThemeService _theme;
        private readonly IBackgroundTaskService _background;

        public ICommand LaunchLogFolder { get; }
        public ICommand StartupStateCommand { get; }

        public string Version { get; private set; }

        private bool _showToast;
        public bool ShowToast
        {
            get => _showToast;
            set
            {
                _showToast = value;
                _settings.SetShowNotificationToasts(value);
                NotifyPropertyChanged(nameof(ShowToast));
            }
        }

        private bool _showBadge;
        public bool ShowBadge
        {
            get => _showBadge;
            set
            {
                _showBadge = value;
                _settings.SetShowBadge(value);
                _badge.Refresh();
                NotifyPropertyChanged(nameof(ShowBadge));
            }
        }

        private bool _aggregateToasts;
        public bool AggregateToasts
        {
            get => _aggregateToasts;
            set
            {
                _aggregateToasts = value;
                _settings.SetAggregateNotificationToasts(value);
            }
        }

        private bool _allowMeteredConnection;
        public bool AllowMeteredConnection
        {
            get => _allowMeteredConnection;
            set
            {
                _allowMeteredConnection = value;
                _settings.SetAllowMeteredConnection(value);
                _messenger.PublishAsync(new UpdateSynchronizationState()).FireAndForgetSafeAsync();
            }
        }

        private bool _backgroundTasksAllowed;
        public bool BackgroundTasksAllowed
        {
            get => _backgroundTasksAllowed;
        }

        private bool _synchronizationEnabled;
        public bool SynchronizationEnabled
        {
            get => _synchronizationEnabled;
            set => ToggleSynchronization(value);
        }

        private void ToggleSynchronization(bool value)
        {
            _settings.SetSynchronizationEnabled(value);

            var enabled = _background.ToggleTask(Constants.Task.InProc.Sync);

            _synchronizationEnabled = enabled;
            NotifyPropertyChanged(nameof(SynchronizationEnabled));
        }

        private StartupState _state;
        private string _startupStateMessage;
        public string StartupStateMessage
        {
            get => _startupStateMessage;
            set
            {
                _startupStateMessage = value;
                NotifyPropertyChanged(nameof(StartupStateMessage));
            }
        }

        private string _startupStateActionMessage;
        public string StartupStateActionText
        {
            get => _startupStateActionMessage;
            set
            {
                _startupStateActionMessage = value;
                NotifyPropertyChanged(nameof(StartupStateActionText));
            }
        }

        private bool _scrollToLastComment;
        public bool ScrollToLastComment
        {
            get => _scrollToLastComment;
            set
            {
                _scrollToLastComment = value;
                _settings.SetScrollToLastComment(value);
                NotifyPropertyChanged(nameof(ScrollToLastComment));
            }
        }

        private bool _markMutedAsRead;
        public bool MarkMutedAsRead
        {
            get => _markMutedAsRead;
            set
            {
                _markMutedAsRead = true;
                _settings.SetMarkMutedAsRead(value);
                NotifyPropertyChanged(nameof(MarkMutedAsRead));
            }
        }

        private bool _automaticallyMarkAsRead;
        public bool AutomaticallyMarkAsRead
        {
            get => _automaticallyMarkAsRead;
            set
            {
                _automaticallyMarkAsRead = value;
                _settings.SetAutomaticallyMarkNotificationsAsRead(value);
                NotifyPropertyChanged(nameof(AutomaticallyMarkAsRead));
            }
        }

        private bool _synchronizeUnreadState;
        public bool SynchronizeUnreadState
        {
            get => _synchronizeUnreadState;
            set
            {
                _synchronizeUnreadState = value;
                _settings.SetSynchronizeUnreadState(value);
                NotifyPropertyChanged(nameof(SynchronizeUnreadState));
            }
        }

        private bool _synchronizeAvatars;
        public bool SynchronizeAvatars
        {
            get => _synchronizeAvatars;
            set
            {
                _synchronizeAvatars = value;
                _settings.SetSynchronizeAvatars(value);
                NotifyPropertyChanged(nameof(SynchronizeAvatars));
            }
        }

        private bool _showAvatars;
        public bool ShowAvatars
        {
            get => _showAvatars;
            set
            {
                _showAvatars = value;
                _settings.SetShowAvatars(value);
                NotifyPropertyChanged(nameof(ShowAvatars));
            }
        }

        private bool _preferInternetAvatars;
        public bool PreferInternetAvatars
        {
            get => _preferInternetAvatars;
            set
            {
                _preferInternetAvatars = value;
                _settings.SetPreferInternetAvatars(value);
                NotifyPropertyChanged(nameof(PreferInternetAvatars));
            }
        }

        public List<GhostlyTheme> Themes { get; }
        public Stateful<GhostlyTheme> CurrentTheme { get; }

        public List<LogLevel> LogLevels { get; }
        public Stateful<LogLevel> LogLevel { get; }

        public List<Culture> Cultures { get; }
        public Stateful<Culture> CurrentCulture { get; }

        public Stateful<bool> NeedsRestart { get; } = new Stateful<bool>(false);

        public SettingsViewModel(
            IPackageService package,
            ILocalSettings settings,
            IMessageService messenger,
            IFolderLauncher folderLauncher,
            ILogLevelSwitch logLevelSwitch,
            IBadgeUpdater badge,
            IThemeService theme,
            IBackgroundTaskService background,
            IStartupHelper startup,
            ICultureService languages,
            ILocalizer localizer)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _folderLauncher = folderLauncher ?? throw new ArgumentNullException(nameof(folderLauncher));
            _logLevelSwitch = logLevelSwitch ?? throw new ArgumentNullException(nameof(logLevelSwitch));
            _badge = badge ?? throw new ArgumentNullException(nameof(badge));
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
            _background = background ?? throw new ArgumentNullException(nameof(background));
            _startup = startup ?? throw new ArgumentNullException(nameof(startup));
            _cultures = languages ?? throw new ArgumentNullException(nameof(languages));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

            Version = string.Empty;
            LaunchLogFolder = new AsyncRelayCommand(() => _folderLauncher.LaunchLogFolder());
            LogLevels = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToList();
            Themes = Enum.GetValues(typeof(GhostlyTheme)).Cast<GhostlyTheme>().ToList();
            Cultures = new List<Culture>();
            Version = _package.GetVersion();

            LogLevel = new Stateful<LogLevel>(value =>
            {
                _settings.SetLogLevel(value);
                _logLevelSwitch.SetMinimumLevel(value);
            });

            CurrentTheme = new Stateful<GhostlyTheme>(t => _theme.SetTheme(t));
            CurrentCulture = new Stateful<Culture>(value =>
            {
                _settings.SetCurrentCulture(value.CultureCode);
                NeedsRestart.Value = true;
            });

            // Update settings
            _showToast = _settings.GetShowNotificationToasts();
            _showBadge = _settings.GetShowBadge();
            _aggregateToasts = _settings.GetAggregateNotificationToasts();
            _allowMeteredConnection = _settings.GetAllowMeteredConnection();
            _scrollToLastComment = _settings.GetScrollToLastComment();
            _synchronizeUnreadState = _settings.GetSetSynchronizeUnreadState();
            _markMutedAsRead = _settings.GetMarkMutedAsRead();
            _automaticallyMarkAsRead = _settings.GetAutomaticallyMarkNotificationsAsRead();
            _synchronizeAvatars = _settings.GetSynchronizeAvatars();
            _showAvatars = _settings.GetShowAvatars();
            _preferInternetAvatars = _settings.GetPreferInternetAvatars();

            StartupStateCommand = new AsyncRelayCommand(async () =>
            {
                var state = await _startup.GetState();
                if (state == StartupState.Disabled)
                {
                    await _startup.Enable();
                }
                else
                {
                    await _startup.Disable();
                }

                await UpdateStateMessage();
            },
            () =>
            {
                return _state == StartupState.Enabled || _state == StartupState.Disabled;
            });
        }

        public Task<bool> Initialize(bool background)
        {
            if (!background)
            {
                // Get the current loglevel.
                LogLevel.Initialize(_settings.GetLogLevel());

                // Get the current theme.
                CurrentTheme.Initialize(_theme.Current);

                // Get supported cultures.
                Cultures.AddRange(_cultures.GetSupportedCultures());
                CurrentCulture.Initialize(_cultures.Current);

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        protected override async Task OnInitialize()
        {
            // Update computed settings dependent on other services.
            _synchronizationEnabled = _background.IsRegistered(Constants.Task.InProc.Sync);
            _backgroundTasksAllowed = _background.BackgroundTasksAllowed;

            await UpdateStateMessage();
        }

        protected override async Task OnActivate()
        {
            await UpdateStateMessage();
        }

        private async Task UpdateStateMessage()
        {
            try
            {
                UpdateStateMessage(await _startup.GetState());
            }
            catch
            {
                UpdateStateMessage(StartupState.Error);
            }
        }

        private void UpdateStateMessage(StartupState state)
        {
            _state = state;

            switch (_state)
            {
                case StartupState.Error:
                    StartupStateMessage = _localizer["Settings_StartupStateError"]; // "An error occured while retrieving startup state.";
                    StartupStateActionText = _localizer["Settings_EnableAutomaticStartup"]; // "Enable automatic startup";
                    break;
                case StartupState.Enabled:
                    StartupStateMessage = _localizer["Settings_StartupStateEnabled"]; // "Automatic startup is enabled.";
                    StartupStateActionText = _localizer["Settings_DisableAutomaticStartup"]; // "Disable automatic startup";
                    break;
                case StartupState.EnabledByPolicy:
                    StartupStateMessage = _localizer["Settings_StartupStateEnabledByPolicy"]; // "Automatic startup is enabled by group policy.";
                    StartupStateActionText = _localizer["Settings_DisableAutomaticStartup"]; // "Disable automatic startup";
                    break;
                case StartupState.Disabled:
                    StartupStateMessage = _localizer["Settings_StartupStateDisabled"]; // "Automatic startup is disabled.";
                    StartupStateActionText = _localizer["Settings_EnableAutomaticStartup"]; // "Enable automatic startup";
                    break;
                case StartupState.DisabledByUser:
                    StartupStateMessage = _localizer["Settings_StartupStateDisabledByUser"]; // "Automatic startup is disabled by user. You can enable this in the Startup tab in Task Manager.";
                    StartupStateActionText = _localizer["Settings_EnableAutomaticStartup"]; // "Enable automatic startup";
                    break;
                case StartupState.DisabledByPolicy:
                    StartupStateMessage = _localizer["Settings_StartupStateDisabledByPolicy"]; // "Automatic startup is disabled by group policy.";
                    StartupStateActionText = _localizer["Settings_EnableAutomaticStartup"]; // "Enable automatic startup";
                    break;
            }

            NotifyPropertyChanged(nameof(StartupStateCommand));
        }
    }
}

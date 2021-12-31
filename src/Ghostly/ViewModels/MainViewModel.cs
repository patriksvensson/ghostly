using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Mvvm;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Features.Notifications;
using Ghostly.Services;
using Ghostly.ViewModels.Commands;
using MediatR;

namespace Ghostly.ViewModels
{
    [DependentOn(typeof(ICategoryService))]
    [DependentOn(typeof(DatabaseInitializer))]
    public sealed partial class MainViewModel : Screen, IInitializable
    {
        private readonly INotificationService _service;
        private readonly ICategoryService _categories;
        private readonly ISynchronizationService _synchronizer;
        private readonly ITemplateService _template;
        private readonly IUriLauncher _uriLauncher;
        private readonly ILocalSettings _settings;
        private readonly IMediator _mediator;
        private readonly ILocalizer _localizer;
        private readonly IGhostlyLog _log;

        private Notification _selectedItem;
        private IList<Notification> _selectedItems;
        private bool _automaticallyMarkNotificationsAsRead;

        public event EventHandler QueryCleared = (s, e) => { };
        public event EventHandler<SynchronizationStateChanged> SynchronizationStateChanged = (s, e) => { };

        public NotificationSource Notifications { get; }
        public bool IsMultipleSelected => SelectedItems?.Count > 1;
        public string SearchTitle => GetSearchTitle();
        public bool ShowCategoryTags => Notifications.Category?.Kind == CategoryKind.Filter || Notifications.SearchMode;

        public ICommand OpenInBrowserCommand { get; }
        public ICommand SyncSingleCommand { get; }
        public ICommand SyncCommand { get; }
        public ICommand ArchiveCommand { get; }
        public ICommand MarkAsReadCommand { get; }
        public ICommand MoveCommand { get; }
        public ICommand MuteCommand { get; }
        public ICommand UnmuteCommand { get; }
        public ICommand StarCommand { get; }
        public ICommand UnstarCommand { get; }

        public Notification SelectedItem
        {
            get => _selectedItem;
            set
            {
                var old = _selectedItem;
                _selectedItem = value;

                OnSelectionChanged(old, _selectedItem);

                if (SelectedItem != null)
                {
                    NotifyPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "By design")]
        public IList<Notification> SelectedItems
        {
            get => _selectedItems;
            set
            {
                _selectedItems = value;

                // Refresh UI.
                NotifyPropertyChanged(nameof(MoveCommand));
                NotifyPropertyChanged(nameof(ArchiveCommand));
                NotifyPropertyChanged(nameof(MuteCommand));
                NotifyPropertyChanged(nameof(UnmuteCommand));
                NotifyPropertyChanged(nameof(StarCommand));
                NotifyPropertyChanged(nameof(UnstarCommand));
                NotifyPropertyChanged(nameof(MarkAsReadCommand));
                NotifyPropertyChanged(nameof(IsMultipleSelected));
                NotifyPropertyChanged(nameof(SyncSingleCommand));
            }
        }

        public MainViewModel(
            INotificationService service,
            ICategoryService categories,
            ISynchronizationService synchronizer,
            ITemplateService template,
            IMessageService messenger,
            IUriLauncher uriLauncher,
            ILocalSettings settings,
            IMediator mediator,
            ILocalizer localizer,
            IGhostlyLog log)
        {
            _service = service;
            _categories = categories;
            _synchronizer = synchronizer;
            _template = template;
            _uriLauncher = uriLauncher;
            _settings = settings;
            _mediator = mediator;
            _localizer = localizer;
            _log = log;

            Notifications = new NotificationSource(_mediator, _log);

            // Commands
            OpenInBrowserCommand = new OpenInBrowserCommand(_uriLauncher);
            SyncSingleCommand = new SyncSingleCommand(_synchronizer);
            SyncCommand = new SyncCommand(_synchronizer);
            MarkAsReadCommand = new MarkAsReadCommand(this, _service);
            ArchiveCommand = new ArchiveCommand(this, Notifications, _service);
            MoveCommand = new MoveCommand(this, _service);
            MuteCommand = new MuteCommand(this, _service);
            UnmuteCommand = new UnmuteCommand(this, _service);
            StarCommand = new StarCommand(this, _service);
            UnstarCommand = new UnstarCommand(this, _service);

            // Subscribe to change events.
            messenger.Subscribe<ApplicationActivated>(OnApplicationActivated);
            messenger.SubscribeOnUIThread<RefreshApplication>(OnRefreshApplication);
            messenger.SubscribeOnUIThreadAsync<NewNotifications>(OnNewNotifications);
            messenger.SubscribeOnUIThreadAsync<NotificationStateChanged>(OnNotificationStateChanged);
            messenger.SubscribeOnUIThread<NotificationRead>(OnNotificationRead);
            messenger.SubscribeOnUIThread<NotificationMuted>(OnNotificationMuted);
            messenger.SubscribeOnUIThread<NotificationUnmuted>(OnNotificationUnmuted);
            messenger.SubscribeOnUIThread<NotificationStarred>(OnNotificationStarred);
            messenger.SubscribeOnUIThread<NotificationUnstarred>(OnNotificationUnstarred);
            messenger.SubscribeOnUIThreadAsync<NotificationMoved>(OnNotificationMoved);
            messenger.SubscribeOnUIThread<NotificationViewStateChanged>(OnNotificationViewStateChanged);
            messenger.SubscribeOnUIThreadAsync<SynchronizationAvailabilityChanged>(OnSynchronizationAvailabilityChanged);
            messenger.SubscribeOnUIThreadAsync<SynchronizationStateChanged>(OnSynchronizationStateChanged);
            messenger.SubscribeOnUIThreadAsync<CategoryDeleted>(OnCategoryDeleted);
            messenger.SubscribeOnUIThreadAsync<CategoryEdited>(OnCategoryEdited);
            messenger.SubscribeOnUIThread<SettingUpdated>(OnSettingUpdated);
            messenger.SubscribeOnUIThread<RefreshNotifications>(OnRefreshNotifications);
        }

        public Task<bool> Initialize(bool background)
        {
            _automaticallyMarkNotificationsAsRead = _settings.GetAutomaticallyMarkNotificationsAsRead();

            Notifications.Category = _categories.Categories.FirstOrDefault(x => x.Inbox);
            Notifications.Reset();

            return Task.FromResult(true);
        }

        public Task OnLinkClicked(Uri uri)
        {
            return _uriLauncher.Launch(uri);
        }

        public Task UpdateFilter(NotificationReadFilter filter)
        {
            Notifications.Filter = filter;
            Notifications.Reset();
            return Task.CompletedTask;
        }

        public Task OnQuery(string text)
        {
            // Nothing to reset?
            if (string.IsNullOrWhiteSpace(text) &&
                string.IsNullOrWhiteSpace(Notifications.Query))
            {
                return Task.CompletedTask;
            }

            Notifications.Query = string.IsNullOrWhiteSpace(text) ? null : text;
            Notifications.Reset();
            return Task.CompletedTask;
        }

        public async Task OnNewNotifications(NewNotifications message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await Notifications.UpdateNotifications(message.Notifications);
            NotifyPropertyChanged(nameof(MarkAsReadCommand));

            if (SelectedItem != null)
            {
                var sameItem = message.Notifications.FirstOrDefault(x => x.Id == SelectedItem.Id);
                if (sameItem != null)
                {
                    SelectedItem = null;
                    SelectedItem = sameItem;
                }
            }
        }

        public void OnNotificationViewStateChanged(NotificationViewStateChanged message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // Clear the current query.
            Notifications.Query = null;
            QueryCleared(this, EventArgs.Empty);

            Notifications.SearchMode = message.Search;
            Notifications.Category = _categories.Categories.FirstOrDefault(x => x.Id == message.CategoryId);
            Notifications.Reset();

            NotifyPropertyChanged(nameof(SearchTitle));
            NotifyPropertyChanged(nameof(ArchiveCommand));
            NotifyPropertyChanged(nameof(ShowCategoryTags));
        }

        public async Task<NotificationViewModel> GetNotificationDetails(Notification notification)
        {
            if (notification is null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            _log.Verbose("Getting work item for notification {NotificationId}...", notification.Id);
            var workitem = await _mediator.Send(new GetWorkItemHandler.Request(notification.WorkItemId, track: false));
            if (workitem == null)
            {
                _log.Verbose("Work item for notification {NotificationId} could not be found.", notification.Id);
                return null;
            }

            var html = await _template.RenderNotification(notification, workitem);
            _log.Verbose("Done rendering notification {NotificationId}.", notification.Id);

            return new NotificationViewModel
            {
                Notification = notification,
                WorkItem = workitem,
                Html = html,
            };
        }

        public void MarkSelectedAsRead()
        {
            if (SelectedItem != null)
            {
                if (MarkAsReadCommand.CanExecute(SelectedItem))
                {
                    MarkAsReadCommand.Execute(SelectedItem);
                }
            }
        }

        private void OnApplicationActivated(ApplicationActivated message)
        {
            if (!message.IsBackgroundActivated)
            {
                _synchronizer.Trigger().FireAndForgetSafeAsync();
            }
        }

        private Task OnSynchronizationStateChanged(SynchronizationStateChanged message)
        {
            NotifyPropertyChanged(nameof(SyncCommand));
            NotifyPropertyChanged(nameof(SyncSingleCommand));
            SynchronizationStateChanged(this, message);
            return Task.CompletedTask;
        }

        private Task OnCategoryDeleted(CategoryDeleted message)
        {
            if (!Notifications.SearchMode)
            {
                if (Notifications.Category != null && Notifications.Category.Id == message.Category.Id)
                {
                    // Select the default category.
                    Notifications.Category = _categories.Categories.Single(x => x.Inbox);
                    Notifications.Reset();
                }
            }

            return Task.CompletedTask;
        }

        private string GetSearchTitle()
        {
            if (Notifications.SearchMode)
            {
                return _localizer.GetString("Main_SearchEverywhere");
            }

            if (Notifications.Category != null)
            {
                return _localizer.Format("Main_SearchInCategory", Notifications.Category.Name);
            }

            // Not sure why category is null here sometimes, but
            // when it is, fall back to use "Search" as a work around
            // until this has been fixed...
            return string.Empty;
        }

        private Task OnCategoryEdited(CategoryEdited message)
        {
            if (!Notifications.SearchMode)
            {
                if (Notifications.Category != null && Notifications.Category.Id == message.Category.Id)
                {
                    // Select the default category.
                    Notifications.Category = _categories.Categories.Single(x => x.Id == message.Category.Id);
                    Notifications.Reset();
                }
            }

            return Task.CompletedTask;
        }

        private Task OnSynchronizationAvailabilityChanged(SynchronizationAvailabilityChanged message)
        {
            NotifyPropertyChanged(nameof(SyncCommand));
            NotifyPropertyChanged(nameof(SyncSingleCommand));
            return Task.CompletedTask;
        }

        private void OnRefreshApplication(RefreshApplication message)
        {
            Notifications.Reset();
        }

        private void OnRefreshNotifications(RefreshNotifications message)
        {
            Notifications.Reset();
        }

        private void OnSelectionChanged(Notification old, Notification @new)
        {
            // Mark the old item as read.
            if (_automaticallyMarkNotificationsAsRead)
            {
                if (@new != null && old != null && old.Unread)
                {
                    _service.MarkAsRead(new[] { old }).FireAndForgetSafeAsync();
                }
            }
        }

        private void OnNotificationRead(NotificationRead message)
        {
            foreach (var notification in message.Notifications)
            {
                var model = Notifications.FirstOrDefault(i => i.Id == notification.Id);
                if (model != null)
                {
                    model.Unread = false;
                }
            }

            NotifyPropertyChanged(nameof(MarkAsReadCommand));
        }

        private async Task OnNotificationMoved(NotificationMoved message)
        {
            // Get all items that are visible in the current category.
            var ids = message.Notifications.Select(x => x.Id).ToArray();
            var visible = await _mediator.Send(new CheckVisibilityHandler.Request(ids, Notifications.Category));

            foreach (var id in ids)
            {
                // Get the item from the notification list.
                var notification = Notifications.SingleOrDefault(i => i.Id == id);
                if (notification != null)
                {
                    // Update the category.
                    notification.Category.Id = message.Category.Id;
                    notification.Category.Name = message.Category.Name;
                    notification.Category.Archive = message.Category.Archive;

                    // Not visible?
                    if (!visible.Contains(id))
                    {
                        Notifications.Remove(notification);
                    }
                }
            }
        }

        private async Task OnNotificationStateChanged(NotificationStateChanged message)
        {
            // Get all items that are visible in the current category.
            var visible = await _mediator.Send(new CheckVisibilityHandler.Request(
                message.Notifications, Notifications.Category));

            foreach (var id in message.Notifications)
            {
                var notification = Notifications.SingleOrDefault(i => i.Id == id);
                if (notification != null)
                {
                    if (notification.IsInState(message.State))
                    {
                        continue;
                    }

                    if (message.State == UpdateNotificationState.Mute ||
                        message.State == UpdateNotificationState.Unmute)
                    {
                        notification.Muted = message.State == UpdateNotificationState.Mute;
                        if (visible.Contains(notification.Id))
                        {
                            // The current category shows starred items, so just continue.
                            continue;
                        }
                    }

                    if (message.State == UpdateNotificationState.Star || message.State == UpdateNotificationState.Unstar)
                    {
                        notification.Starred = message.State == UpdateNotificationState.Star;
                        if (visible.Contains(notification.Id))
                        {
                            // The current category shows starred items, so just continue.
                            continue;
                        }
                    }

                    var index = Notifications.IndexOf(notification);
                    if (index != -1)
                    {
                        Notifications.Remove(notification);
                        if (Notifications.Count > 0)
                        {
                            // Select the next item (or previous if this was the last item).
                            index = Math.Max(0, Math.Min(index, Notifications.Count - 1));
                            SelectedItem = Notifications[index];
                        }
                    }
                }
            }

            // Performs a manual refresh in an attempt to load more items.
            if (Notifications.Count == 0)
            {
                Notifications.Reset();
            }
        }

        private void OnNotificationUnmuted(NotificationUnmuted message)
        {
            NotifyPropertyChanged(nameof(MuteCommand));
            NotifyPropertyChanged(nameof(UnmuteCommand));
        }

        private void OnNotificationMuted(NotificationMuted message)
        {
            NotifyPropertyChanged(nameof(MuteCommand));
            NotifyPropertyChanged(nameof(UnmuteCommand));
        }

        private void OnNotificationStarred(NotificationStarred message)
        {
            NotifyPropertyChanged(nameof(StarCommand));
            NotifyPropertyChanged(nameof(UnstarCommand));
        }

        private void OnNotificationUnstarred(NotificationUnstarred message)
        {
            NotifyPropertyChanged(nameof(StarCommand));
            NotifyPropertyChanged(nameof(UnstarCommand));
        }

        private void OnSettingUpdated(SettingUpdated message)
        {
            if (message.SettingName.Equals(Constants.Settings.AutomaticallyMarkNotificationsAsRead, StringComparison.OrdinalIgnoreCase))
            {
                _automaticallyMarkNotificationsAsRead = _settings.GetAutomaticallyMarkNotificationsAsRead();
            }
        }
    }
}

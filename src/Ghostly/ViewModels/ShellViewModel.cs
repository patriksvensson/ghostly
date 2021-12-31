using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ghostly.Core;
using Ghostly.Core.Mvvm;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Services;
using Ghostly.ViewModels.Commands;
using MediatR;

namespace Ghostly.ViewModels
{
    [DependentOn(typeof(DatabaseInitializer))]
    [DependentOn(typeof(IUnreadService))]
    [DependentOn(typeof(ICategoryService))]
    [DependentOn(typeof(ICultureService))]
    public sealed partial class ShellViewModel : Screen, IViewModel, IInitializable
    {
        private readonly INavigationService _navigator;
        private readonly ICategoryService _categories;
        private readonly IMessageService _messenger;
        private readonly ISynchronizationService _synchronizer;
        private readonly IUnreadService _unread;
        private readonly IBadgeUpdater _badge;
        private readonly IShellResolver _shellResolver;

        public ICommand CreateCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand ArchiveCategoryCommand { get; }
        public ICommand MarkCategoryAsReadCommand { get; }

        public NavigationViewModel Navigation { get; }
        public ProgressViewModel Status { get; }
        public ProgressViewModel Progress { get; }
        public Stateful<bool> AppDisabled { get; }

        public bool CanSynchronize => _synchronizer.CanSynchronize;

        public ShellViewModel(
            INavigationService navigator,
            IDialogService dialogs,
            ICategoryService categories,
            IMessageService messenger,
            ISynchronizationService synchronizer,
            IUnreadService unread,
            IBadgeUpdater badge,
            IMediator mediator,
            ILocalizer localizer,
            IShellResolver viewResolver)
        {
            _navigator = navigator;
            _categories = categories;
            _messenger = messenger;
            _synchronizer = synchronizer;
            _unread = unread;
            _badge = badge;
            _shellResolver = viewResolver;

            CreateCategoryCommand = new CreateCategoryCommand(dialogs, categories);
            EditCategoryCommand = new EditCategoryCommand(dialogs, categories);
            DeleteCategoryCommand = new DeleteCategoryCommand(mediator, localizer, dialogs, categories);
            ArchiveCategoryCommand = new ArchiveCategoryCommand(mediator, localizer, dialogs);
            MarkCategoryAsReadCommand = new MarkCategoryAsReadCommand(mediator, localizer, dialogs);

            Navigation = new NavigationViewModel(unread, localizer, NavigationItemChanged);
            Status = new ProgressViewModel();
            Progress = new ProgressViewModel(show: false);
            AppDisabled = new Stateful<bool>(false);
        }

        Task<bool> IInitializable.Initialize(bool background)
        {
            if (!background)
            {
                // Subscribe to messages.
                _messenger.SubscribeOnUIThread<RefreshApplication>(OnRefreshApplication);
                _messenger.SubscribeOnUIThreadAsync<SynchronizationAvailabilityChanged>(OnSynchronizationAvailabilityChanged);
                _messenger.SubscribeOnUIThread<UnreadCountChanged>(OnUnreadCountChanged);
                _messenger.SubscribeOnUIThread<InAppNotification>(OnInAppNotification);
                _messenger.SubscribeOnUIThread<StatusMessage>(OnStatusMessage);
                _messenger.SubscribeOnUIThread<ShowProgress>(OnShowProgress);
                _messenger.SubscribeOnUIThreadAsync<CategoryCreated>(OnCategoryCreated);
                _messenger.SubscribeOnUIThreadAsync<CategoryEdited>(OnCategoryEdited);
                _messenger.SubscribeOnUIThreadAsync<CategoryDeleted>(OnCategoryDeleted);
                _messenger.SubscribeOnUIThread<AccountProblemDetected>(OnAccountProblemDetected);
                _messenger.SubscribeOnUIThread<AccountProblemResolved>(OnAccountProblemResolved);
                _messenger.SubscribeOnUIThread<GhostlyDisabled>(OnAppDisabled);

                // Initialize categories.
                Navigation.Initialize(_categories.Categories);
            }

            // We're not fully initialized yet.
            return Task.FromResult(!background);
        }

        private void NavigationItemChanged(NavigationItem item)
        {
            if (item != null)
            {
                if (item.IsCategory)
                {
                    Navigation.Menu.Selected.Value = null;
                    Navigate(new NavigationItemInvokedEventArgs(item.Kind, item.Id.Value))
                        .FireAndForgetSafeAsync();
                }
                else if (item.IsCommand)
                {
                    Navigation.Commands.Selected.Value = null;
                    if (item.Kind == NavigationItemKind.NewCategory)
                    {
                        CreateCategoryCommand.Execute(null);
                    }
                }
                else if (item.IsMenu)
                {
                    Navigation.Categories.Selected.Value = null;
                    Navigate(new NavigationItemInvokedEventArgs(item.Kind))
                        .FireAndForgetSafeAsync();
                }
            }
        }

        protected override Task OnViewInitialized()
        {
            // Update item count.
            OnUnreadCountChanged(new UnreadCountChanged
            {
                UnreadCount = _unread.UnreadCount,
                TotalCount = _unread.TotalCount,
            });

            return Task.CompletedTask;
        }

        public async Task ReorderCategories(IEnumerable<CategoryOrder> ordering)
        {
            await _categories.ReorderCategories(ordering);
        }

        public async Task NavigateToInbox()
        {
            var defaultCategory = Navigation.GetInbox();
            await Navigate(new NavigationItemInvokedEventArgs(NavigationItemKind.Category, defaultCategory.Id.Value, true));
        }

        public async Task Navigate(NavigationItemInvokedEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            switch (e.Kind)
            {
                case NavigationItemKind.Inbox:
                    var inbox = _categories.Categories.Single(p => p.Inbox);
                    await _messenger.PublishAsync(new NotificationViewStateChanged(inbox.Id));
                    await _navigator.Navigate<MainViewModel>(e);
                    break;
                case NavigationItemKind.Category:
                    await _messenger.PublishAsync(new NotificationViewStateChanged(e.Id.Value));
                    await _navigator.Navigate<MainViewModel>(e);
                    break;
                case NavigationItemKind.Search:
                    await _messenger.PublishAsync(new NotificationViewStateChanged(null, true));
                    await _navigator.Navigate<MainViewModel>(e);
                    break;
                case NavigationItemKind.Rules:
                    await _navigator.Navigate<RuleViewModel>(e);
                    break;
                case NavigationItemKind.Account:
                    await _navigator.Navigate<AccountViewModel>(e);
                    break;
                case NavigationItemKind.Settings:
                    await _navigator.Navigate<SettingsViewModel>(e);
                    break;
            }
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core.Mvvm;
using Ghostly.Domain.Messages;

namespace Ghostly.ViewModels
{
    public sealed partial class ShellViewModel
    {
        private Task OnSynchronizationAvailabilityChanged(SynchronizationAvailabilityChanged message)
        {
            NotifyPropertyChanged(nameof(CanSynchronize));
            return Task.CompletedTask;
        }

        private void OnRefreshApplication(RefreshApplication message)
        {
            NotifyPropertyChanged(nameof(CanSynchronize));
        }

        private void OnInAppNotification(InAppNotification message)
        {
            _shellResolver.GetShell().ShowInAppNotification(message.Message, message.Timeout);
        }

        private void OnUnreadCountChanged(UnreadCountChanged message)
        {
            _badge.Update(message.TotalUnread);

            foreach (var category in Navigation.Categories)
            {
                if (category.Muted)
                {
                    category.Count = 0;
                    continue;
                }

                // Show total count?
                if (category.Category.ShowTotal)
                {
                    if (message.TotalCount.TryGetValue(category.Id.Value, out var count))
                    {
                        category.Count = count;
                    }
                    else
                    {
                        category.Count = 0;
                    }
                }
                else
                {
                    if (message.UnreadCount.TryGetValue(category.Id.Value, out var count))
                    {
                        category.Count = count;
                    }
                    else
                    {
                        category.Count = 0;
                    }
                }
            }
        }

        private void OnStatusMessage(StatusMessage message)
        {
            Status.Update(message.Message, message.Percentage);
        }

        private void OnShowProgress(ShowProgress message)
        {
            Progress.Update(message.Message, message.Percentage);
        }

        private async Task OnCategoryCreated(CategoryCreated message)
        {
            Navigation.Categories.Add(new NavigationItem
            {
                Id = message.Category.Id,
                Category = message.Category,
                IsDeletable = message.Category.Deletable,
                Count = 0,
                Name = message.Category.Name,
                Kind = NavigationItemKind.Category,
                Glyph = message.Category.Glyph,
                Emoji = message.Category.Emoji,
                Muted = message.Category.Muted,
            });

            await _messenger.PublishAsync(new UpdateUnreadCount());
        }

        private async Task OnCategoryEdited(CategoryEdited message)
        {
            var category = Navigation.Categories.Find(message.Category.Id);
            if (category != null)
            {
                category.Name = message.Category.Name;
                category.Glyph = message.Category.Glyph;
                category.Emoji = message.Category.Emoji;
                category.Category = message.Category;
                category.Muted = message.Category.Muted;

                await _messenger.PublishAsync(new UpdateUnreadCount());
            }
        }

        private async Task OnCategoryDeleted(CategoryDeleted message)
        {
            // Navigate to the inbox.
            NavigateToInbox().FireAndForgetSafeAsync();

            // Remove the category.
            Navigation.Categories.Remove(Navigation.Categories.SingleOrDefault(c => c.Id == message.Category.Id));

            // Update the unread count.
            await _messenger.PublishAsync(new UpdateUnreadCount());
        }

        private void OnAccountProblemDetected(AccountProblemDetected message)
        {
            var item = Navigation.Menu.Find(NavigationItemKind.Account);
            if (item != null)
            {
                item.HasProblem = true;
            }
        }

        private void OnAccountProblemResolved(AccountProblemResolved message)
        {
            var item = Navigation.Menu.Find(NavigationItemKind.Account);
            if (item != null)
            {
                item.HasProblem = false;
            }
        }

        private void OnAppDisabled(GhostlyDisabled message)
        {
            AppDisabled.Value = message.Disabled;
        }
    }
}

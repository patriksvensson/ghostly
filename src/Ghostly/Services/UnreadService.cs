using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Domain.Messages;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Services
{
    public interface IUnreadService
    {
        IReadOnlyDictionary<int, int> TotalCount { get; }
        IReadOnlyDictionary<int, int> UnreadCount { get; }

        Task<int> Update();
    }

    [DependentOn(typeof(DatabaseInitializer))]
    [DependentOn(typeof(ICategoryService))]
    public sealed class UnreadService : IInitializable, IUnreadService
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly ICategoryService _categories;
        private readonly IMessageService _messenger;
        private Dictionary<int, int> _unreadCount;
        private Dictionary<int, int> _totalCount;

        public IReadOnlyDictionary<int, int> TotalCount => _totalCount;
        public IReadOnlyDictionary<int, int> UnreadCount => _unreadCount;

        public UnreadService(
            IGhostlyContextFactory factory,
            ICategoryService categories,
            IMessageService messenger)
        {
            _factory = factory ?? throw new System.ArgumentNullException(nameof(factory));
            _categories = categories ?? throw new System.ArgumentNullException(nameof(categories));
            _messenger = messenger ?? throw new System.ArgumentNullException(nameof(messenger));
            _unreadCount = new Dictionary<int, int>();
            _totalCount = new Dictionary<int, int>();

            // Subscribe for messages
            _messenger.SubscribeAsync<UpdateUnreadCount>(_ => Update());
            _messenger.SubscribeAsync<RefreshApplication>(_ => Update());
            _messenger.SubscribeAsync<NewNotifications>(_ => Update());
            _messenger.SubscribeAsync<NotificationRead>(_ => Update());
            _messenger.SubscribeAsync<NotificationMuted>(_ => Update());
            _messenger.SubscribeAsync<NotificationUnmuted>(_ => Update());
            _messenger.SubscribeAsync<NotificationStateChanged>(_ => Update());
            _messenger.SubscribeAsync<NotificationMoved>(_ => Update());
            _messenger.SubscribeAsync<AccountRemoved>(_ => Update());
            _messenger.SubscribeAsync<AccountUpdated>(_ => Update());
            _messenger.SubscribeAsync<AccountStateChanged>(_ => Update());
            _messenger.SubscribeAsync<RefreshNotifications>(_ => Update());
        }

        public async Task<bool> Initialize(bool background)
        {
            await Update();
            return true;
        }

        public async Task<int> Update()
        {
            var unreadItems = 0;
            var unreadCount = new Dictionary<int, int>();
            var totalCount = new Dictionary<int, int>();

            using (var context = _factory.Create())
            {
                foreach (var account in context.GetActiveAccountQuery())
                {
                    var unreadAccountCount = 0;
                    foreach (var category in _categories.Categories)
                    {
                        if (category.Muted)
                        {
                            // Don't show anything for the category
                            totalCount[category.Id] = 0;
                            unreadCount[category.Id] = 0;
                        }
                        else
                        {
                            if (category.Kind == Domain.CategoryKind.Filter)
                            {
                                // Get the count for the filter.
                                totalCount[category.Id] = await context.GetNotificationQuery()
                                    .Where(n => n.AccountId == account.Id)
                                    .CountAsync(category.Filter);

                                // Get the unread count for the filter.
                                unreadCount[category.Id] = await context.GetNotificationQuery()
                                    .Where(n => n.AccountId == account.Id)
                                    .Where(n => n.Unread && !n.Muted)
                                    .CountAsync(category.Filter);
                            }
                            else
                            {
                                // Get the count for the category.
                                totalCount[category.Id] = await context.GetNotificationQuery()
                                    .Where(n => n.AccountId == account.Id)
                                    .CountAsync(n => n.Category.Id == category.Id);

                                // Get the unread count for the category.
                                unreadCount[category.Id] = await context.GetNotificationQuery()
                                    .Where(n => n.AccountId == account.Id)
                                    .Where(n => n.Unread && !n.Muted)
                                    .CountAsync(n => n.Category.Id == category.Id);

                                // Increase the number of unread items for the account.
                                unreadAccountCount += unreadCount[category.Id];
                            }
                        }
                    }

                    // Increase the total number of unread items.
                    unreadItems += unreadAccountCount;
                }
            }

            // Update the internal representation.
            Interlocked.Exchange(ref _unreadCount, unreadCount);
            Interlocked.Exchange(ref _totalCount, totalCount);

            // Publish the change.
            await _messenger.PublishAsync(new UnreadCountChanged
            {
                TotalUnread = unreadItems,
                TotalCount = totalCount,
                UnreadCount = unreadCount,
            });

            // Return the number of unread items.
            return unreadItems;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Mvvm;
using Ghostly.Domain;
using Ghostly.Features.Notifications;
using MediatR;

namespace Ghostly.Services
{
    public sealed class NotificationSource :
        ObservableCollection<Notification>,
        IIncrementalLoadingSource<Notification>,
        IDisposable
    {
        private readonly IMediator _mediator;
        private readonly IGhostlyLog _log;
        private readonly Semaphore _semaphore;
        private int _count;

        public bool HasMoreItems { get; private set; }
        public event EventHandler Refresh = (s, e) => { };

        public bool SearchMode { get; set; }
        public NotificationReadFilter Filter { get; set; }
        public Category Category { get; set; }
        public string Query { get; set; }

        public NotificationSource(IMediator mediator, IGhostlyLog log)
        {
            _mediator = mediator;
            _log = log;
            Category = null;
            Filter = NotificationReadFilter.All;
            HasMoreItems = true;
            Query = null;
            _semaphore = new Semaphore(1, 1);
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }

        public void Reset()
        {
            _count = 0;
            HasMoreItems = true;

            Refresh(this, EventArgs.Empty);
        }

        public async Task<int> LoadMoreItemsAsync(int count)
        {
            try
            {
                _semaphore.WaitOne();

                var take = Math.Max(count, 25);
                _log.Debug("Loading items: Category={Category}, Filter={Filter}, Query={Query}, Index={Index}, ReqCount={ReqCount}, Count={Count}", Category?.Id, Filter, Query != null, _count, count, take);
                var items = await _mediator.Send(new GetNotificationsHandler.Request(Category, Filter, Query, _count, take, SearchMode));

                _count += items.Count;

                HasMoreItems = items.Count > 0;
                if (!HasMoreItems)
                {
                    _log.Verbose("No more items to load.");
                }

                // Update notifications.
                await UpdateNotifications(items);

                // Returns the number of items.
                return items.Count;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateNotifications(IEnumerable<Notification> items)
        {
            // Get items that's visible.
            var result = await _mediator.Send(new CheckVisibilityHandler.Request(items.Select(x => x.Id), Category));
            foreach (var item in items.Where(x => result.Contains(x.Id)))
            {
                // Update visible items.
                UpdateNotification(item);
            }

            // TODO: Any items in the received list that isn't longer visible but exists in the list?
        }

        private void UpdateNotification(Notification model)
        {
            if (Filter == NotificationReadFilter.Unread && !model.Unread)
            {
                return;
            }

            // We might need to move the item.
            var item = this.FirstOrDefault(x => x.Id == model.Id);
            var timestamp = model.Timestamp;
            var currentPosition = IndexOf(item);

            // Get the new position (which is before the item older than this).
            var newPosition = IndexOf(this.FirstOrDefault(x => x.Timestamp < timestamp));
            if (newPosition == -1)
            {
                if (currentPosition == -1)
                {
                    // No item older than this one. Just add it last.
                    Add(model);
                    return;
                }
                else
                {
                    newPosition = Math.Max(newPosition - 1, 0);
                }
            }
            else
            {
                newPosition = Math.Max(newPosition - 1, 0);
            }

            if (currentPosition != -1)
            {
                if (newPosition != currentPosition)
                {
                    // Move the item.
                    Move(currentPosition, newPosition);

                    // Refresh the item with new information.
                    this[newPosition].Update(model);
                }
                else
                {
                    // Refresh the item with new information.
                    this[newPosition].Update(model);
                }
            }
            else
            {
                // Insert it.
                Insert(newPosition, model);
            }
        }
    }
}

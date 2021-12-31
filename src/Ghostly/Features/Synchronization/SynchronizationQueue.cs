using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Ghostly.Core.Diagnostics;
using Ghostly.Data;
using Ghostly.Data.Models;
using Newtonsoft.Json;

namespace Ghostly.Features.Synchronization
{
    public abstract class SynchronizationQueue<T> : IDisposable
        where T : ISynchronizationItem
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IGhostlyLog _log;
        private readonly SemaphoreSlim _semaphore;
        private readonly HashSet<SynchronizationItem> _cache;
        private bool _disposed;

        public int Count { get; private set; }

        protected virtual IEqualityComparer<T> Comparer { get; }
        protected abstract Discriminator Discriminator { get; }

        protected SynchronizationQueue(
            IGhostlyContextFactory factory,
            IGhostlyLog log)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _semaphore = new SemaphoreSlim(1, 1);
            _cache = new HashSet<SynchronizationItem>();
        }

        protected virtual bool RemoveItemFromCache(T item)
        {
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _semaphore.Dispose();
                }
            }
        }

        protected void Initialize()
        {
            using (var context = _factory.Create())
            {
                var items = context.SyncItems.Where(x => x.Discriminator == Discriminator);
                foreach (var item in items)
                {
                    _cache.Add(new SynchronizationItem
                    {
                        Identity = item.Identity,
                        Timestamp = item.Timestamp.EnsureUniversalTime(),
                    });
                }

                Count = items.Count();
            }
        }

        public bool Contains(T item)
        {
            return _cache.Any(c => c.Identity.Equals(item.Identity, StringComparison.Ordinal)
                && c.Timestamp.Equals(item.Timestamp.EnsureUniversalTime()));
        }

        public void Enqueue(HashSet<T> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (items.Count == 0)
            {
                return;
            }

            try
            {
                _semaphore.Wait();

                // TODO: Enqueue items to the database.
                using (var context = _factory.Create())
                {
                    var updateCount = 0;
                    foreach (var item in items)
                    {
                        if (_cache.Any(c => c.Identity.Equals(item.Identity, StringComparison.Ordinal)
                            && c.Timestamp.Equals(item.Timestamp.EnsureUniversalTime())))
                        {
                            continue;
                        }

                        // Delete same notifications older than this one.
                        DeleteSimilarNotifications(context, item);

                        // Add the notification to the queue.
                        context.SyncItems.Add(new SyncItemData
                        {
                            Discriminator = Discriminator,
                            Identity = item.Identity,
                            Timestamp = item.Timestamp.EnsureUniversalTime(),
                            Payload = JsonConvert.SerializeObject(item),
                        });

                        // Add item to the cache.
                        _cache.Add(new SynchronizationItem
                        {
                            Identity = item.Identity,
                            Timestamp = item.Timestamp.EnsureUniversalTime(),
                        });

                        updateCount++;
                    }

                    if (updateCount > 0)
                    {
                        context.SaveChanges();

                        // Update count.
                        Count += updateCount;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "An error occured while enqueueing notification items.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public bool TryDequeue(int count, out IReadOnlyList<T> result)
        {
            try
            {
                _semaphore.Wait();

                using (var context = _factory.Create())
                {
                    // Get all items.
                    var notifications = context.SyncItems.Where(x => x.Discriminator == Discriminator)
                        .OrderByDescending(x => x.Timestamp)
                        .Take(count);

                    var created = new List<T>();
                    foreach (var notification in notifications)
                    {
                        // Convert the item.
                        var item = JsonConvert.DeserializeObject<T>(notification.Payload);
                        created.Add(item);

                        // Remove the dequeued items from the database.
                        context.SyncItems.Remove(notification);

                        if (RemoveItemFromCache(item))
                        {
                            _cache.RemoveWhere(x => x.Identity.Equals(notification.Identity, StringComparison.Ordinal) &&
                                x.Timestamp.Equals(notification.Timestamp.EnsureUniversalTime()));
                        }
                    }

                    if (created.Count > 0)
                    {
                        // Save changes to the database.
                        context.SaveChanges();

                        // Decrease the created count.
                        Count -= created.Count;

                        // Return success.
                        result = created;
                        return true;
                    }

                    result = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "An error occured while dequeueing notification items.");

                result = null;
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void DeleteSimilarNotifications(GhostlyContext context, T item)
        {
            var itemsToDelete = context.SyncItems.Where(x => x.Discriminator == Discriminator && x.Identity == item.Identity && x.Timestamp < item.Timestamp);
            foreach (var itemToDelete in itemsToDelete)
            {
                context.SyncItems.Remove(itemToDelete);
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Features.Activities;

namespace Ghostly.Jobs
{
    public interface IActivityQueue
    {
        void Add(ActivityPayload payload);
    }

    public sealed class ActivityQueueJob : IBackgroundJob, IActivityQueue, IDisposable
    {
        private readonly BlockingCollection<ActivityData> _queue;
        private readonly IGhostlyContextFactory _factory;
        private readonly IDatabaseLock _lock;
        private bool _disposed;

        public bool Enabled => true;

        public ActivityQueueJob(
            IGhostlyContextFactory factory,
            IDatabaseLock @lock)
        {
            _queue = new BlockingCollection<ActivityData>(new ConcurrentQueue<ActivityData>());
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _lock = @lock ?? throw new ArgumentNullException(nameof(@lock));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _queue.Dispose();
                }

                _disposed = true;
            }
        }

        public void Add(ActivityPayload activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            _queue.Add(new ActivityData
            {
                Timestamp = DateTime.UtcNow,
                Category = activity.Category,
                Kind = activity.Kind,
                Constraint = activity.Contstraint,
                Payload = ActivityHelper.Serialize(activity),
            });
        }

        public async Task<bool> Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var item = _queue.Take(token);
                    if (item != null)
                    {
                        using (await _lock.AcquireWriteLockAsync())
                        {
                            // Add the item to the database.
                            using (var context = _factory.Create())
                            {
                                context.Activities.Add(item);
                                context.SaveChanges();
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }

            return true;
        }
    }
}

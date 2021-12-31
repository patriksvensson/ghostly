using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Features.Activities;

namespace Ghostly.Jobs
{
    public sealed class ActivityProcessingJob : IBackgroundJob
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly ActivityHelper _activity;
        private readonly INetworkHelper _network;
        private readonly IDatabaseLock _lock;
        private readonly IGhostlyLog _log;

        public bool Enabled => true;

        public ActivityProcessingJob(
            IGhostlyContextFactory factory,
            ActivityHelper activity,
            INetworkHelper network,
            IDatabaseLock @lock,
            IGhostlyLog log)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _activity = activity ?? throw new ArgumentNullException(nameof(activity));
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _lock = @lock ?? throw new ArgumentNullException(nameof(@lock));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<bool> Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using (var context = _factory.Create())
                    {
                        // Get all activities.
                        var activities = default(IReadOnlyList<ActivityData>);
                        using (await _lock.AcquireReadLockAsync())
                        {
                            activities = context.Activities
                                .Where(x => x.Category == ActivityCategory.Continuous)
                                .OrderBy(x => x.Timestamp)
                                .ToReadOnlyList();
                        }

                        foreach (var activity in activities)
                        {
                            if (activity.Constraint == ActivityConstraint.RequiresInternetConnection)
                            {
                                // TODO: Respect metered connections.
                                if (!_network.IsConnected)
                                {
                                    continue;
                                }
                            }

                            // Time to bail?
                            if (token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(500)))
                            {
                                break;
                            }

                            var payload = _activity.Deserialize(activity.Kind, activity.Payload);
                            if (payload != null)
                            {
                                if (await _activity.Process(payload, token))
                                {
                                    using (await _lock.AcquireReadLockAsync())
                                    {
                                        context.Activities.Remove(activity);
                                        await context.SaveChangesAsync(true, token);
                                    }
                                }
                            }
                        }
                    }

                    if (token.WaitHandle.WaitOne(TimeSpan.FromSeconds(10)))
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            }

            return true;
        }
    }
}

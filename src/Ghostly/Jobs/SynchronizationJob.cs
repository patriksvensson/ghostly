using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Services;

namespace Ghostly.Jobs
{
    public sealed class SynchronizationJob : IBackgroundJob
    {
        private readonly ISynchronizationService _synchronizer;
        private readonly ILocalSettings _settings;
        private readonly IGhostlyLog _log;

        public bool Enabled => false;

        public SynchronizationJob(
            ISynchronizationService synchronizer,
            ILocalSettings settings,
            IGhostlyLog log)
        {
            _synchronizer = synchronizer;
            _settings = settings;
            _log = log;
        }

        public async Task<bool> Run(CancellationToken token)
        {
            while (true)
            {
                if (!_settings.GetSynchronizationEnabled())
                {
                    if (token.WaitHandle.WaitOne(TimeSpan.FromSeconds(10)))
                    {
                        return true;
                    }

                    continue;
                }

                try
                {
                    // Trigger synchronization.
                    var completed = await _synchronizer.Trigger(token);
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }

                // Wait for a while before synchronizing again.
                if (token.WaitHandle.WaitOne(
                    TimeSpan.FromMinutes(_settings.GetSynchronizationInterval())))
                {
                    break;
                }
            }

            return true;
        }
    }
}

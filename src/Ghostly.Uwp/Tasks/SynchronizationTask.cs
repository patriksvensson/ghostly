using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Services;
using Ghostly.Uwp.Infrastructure;
using Windows.ApplicationModel.Background;

namespace Ghostly.Uwp.Tasks
{
    public sealed class SynchronizationTask : InProcessTask, IDisposable
    {
        private readonly ISynchronizationService _syncer;
        private readonly IUnreadService _unread;
        private readonly IBadgeUpdater _badge;
        private readonly ILocalSettings _settings;
        private readonly ITelemetry _telemetry;
        private readonly IGhostlyLog _log;
        private readonly ManualResetEvent _cancel;

        public override string Name => Constants.Task.InProc.Sync;
        public override bool IsEnabled => _settings.GetSynchronizationEnabled();

        public SynchronizationTask(
            ISynchronizationService syncer,
            IUnreadService unread,
            IBadgeUpdater badge,
            IExtendedExecutionService extendedExecution,
            ILocalSettings settings,
            ITelemetry telemetry,
            IGhostlyLog log)
            : base(extendedExecution, log)
        {
            _syncer = syncer;
            _unread = unread;
            _badge = badge;
            _settings = settings;
            _telemetry = telemetry;
            _log = log;
            _cancel = new ManualResetEvent(false);
        }

        public void Dispose()
        {
            _cancel.Dispose();
        }

        public override void Register(BackgroundTaskBuilder builder)
        {
            builder.SetTrigger(new TimeTrigger(15, false));
        }

        protected override async Task Execute(IBackgroundTaskInstance taskInstance)
        {
            using (_log.Push("Background", true))
            {
                try
                {
                    await _syncer.Trigger();

                    // Update the unread count.
                    // We do this explicitly since we want the badge updated.
                    var unread = await _unread.Update();
                    _badge.Update(unread);
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                    _telemetry.TrackException(ex, nameof(SynchronizationTask));
                }
            }
        }

        public override void Cancel()
        {
            _cancel.Set();
        }
    }
}

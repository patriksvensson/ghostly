using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Uwp.Infrastructure;
using Windows.ApplicationModel.Background;
using Windows.Foundation;

namespace Ghostly.Uwp.Tasks
{
    public abstract class InProcessTask
    {
        private readonly IExtendedExecutionService _extendedExecution;
        private readonly IGhostlyLog _log;

        protected InProcessTask(IExtendedExecutionService extendedExecution, IGhostlyLog log)
        {
            _extendedExecution = extendedExecution;
            _log = log;
        }

        public virtual bool IsLongRunning => true;
        public virtual bool IsEnabled => true;
        public abstract string Name { get; }

        public abstract void Cancel();
        public abstract void Register(BackgroundTaskBuilder builder);

        public async Task Start(IBackgroundTaskInstance task)
        {
            if (task is null)
            {
                throw new System.ArgumentNullException(nameof(task));
            }

            var deferral = default(Deferral);

            using (_log.Push("TaskId", task.InstanceId))
            using (_log.Push("Task", Name))
            {
                if (IsLongRunning)
                {
                    _log.Debug("The task {0} is requesting a session...", Name);
                    if (!await _extendedExecution.RequestSession())
                    {
                        _log.Warning("Can not execute task '{0}' since we got no session.", Name);
                    }

                    _log.Debug("The task '{0}' is trying to get deferral...", Name);
                    deferral = _extendedExecution.GetDeferral();
                }
            }

            using (_log.Push("TaskId", task.InstanceId))
            using (_log.Push("Task", Name))
            {
                await Execute(task);
                deferral?.Complete();
            }
        }

        protected abstract Task Execute(IBackgroundTaskInstance taskInstance);
    }
}

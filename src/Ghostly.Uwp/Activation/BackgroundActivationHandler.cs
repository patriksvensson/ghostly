using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Services;
using Ghostly.Uwp.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;

namespace Ghostly.Uwp.Activation
{
    public sealed class BackgroundActivationHandler : IActivationHandler
    {
        private readonly ILifetimeScope _scope;
        private readonly IBackgroundTaskService _background;
        private readonly IGhostlyLog _log;

        public BackgroundActivationHandler(
            ILifetimeScope scope,
            IBackgroundTaskService background,
            IGhostlyLog log)
        {
            _scope = scope;
            _background = background;
            _log = log;
        }

        public bool CanHandle(object args)
        {
            return args is BackgroundActivatedEventArgs;
        }

        public Task Handle(object args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            Start(((BackgroundActivatedEventArgs)args).TaskInstance);
            return Task.CompletedTask;
        }

        private void Start(IBackgroundTaskInstance taskInstance)
        {
            GhostlyState.IsBackgroundActivated = true;
            _log.Debug("[BackgroundActivationHandler] BackgroundActivated={BackgroundActivated}", GhostlyState.IsBackgroundActivated);

            if (!_background.BackgroundTasksAllowed)
            {
                _log.Information("Background tasks are not allowed.");
                return;
            }

            var name = taskInstance?.Task?.Name;
            var task = _scope.Resolve<IEnumerable<InProcessTask>>().SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (task == null)
            {
                _log.Error("Could not find an in-process task called {BackgroundTaskName}.", name);
                return;
            }

            taskInstance.Canceled += (s, e) =>
            {
                _log.Information("Cancelling task {BackgroundTaskName}: {Reason}", name, e);
                task.Cancel();
            };

            _log.Information("Running task {BackgroundTaskName}...", name);
            task.Start(taskInstance).FireAndForgetSafeAsync();
        }
    }
}

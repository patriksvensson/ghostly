using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Data;
using Ghostly.Services;
using Ghostly.Uwp.Tasks;
using Windows.ApplicationModel.Background;

namespace Ghostly.Uwp.Infrastructure
{
    [DependentOn(typeof(DatabaseInitializer))]
    public sealed class BackgroundTaskService : IInitializable, IBackgroundTaskService
    {
        private readonly ILifetimeScope _scope;
        private readonly ILocalSettings _settings;
        private readonly IGhostlyLog _log;

        public bool BackgroundTasksAllowed { get; private set; }

        public BackgroundTaskService(ILifetimeScope scope, ILocalSettings settings, IGhostlyLog log)
        {
            _scope = scope;
            _settings = settings;
            _log = log;
        }

        public async Task<bool> Initialize(bool background)
        {
#if DEBUG && !REGISTER_BACKGROUND_TASKS
            // Do not register background tasks when debugging.
            await Task.Delay(0);
            return false;
#else
            if (background)
            {
                var accessStatus = BackgroundExecutionManager.GetAccessStatus();
                if (accessStatus.IsAllowed())
                {
                    BackgroundTasksAllowed = true;
                }

                return false;
            }

            var result = await BackgroundExecutionManager.RequestAccessAsync();
            if (result.IsDisallowed())
            {
                _log.Error("Could not register background tasks ({BackgroundAccessStatus}).", result);
                BackgroundTasksAllowed = false;
                return true;
            }

            BackgroundTasksAllowed = true;

            foreach (var task in _scope.ResolveOptional<IEnumerable<InProcessTask>>())
            {
                ToggleTask(task);
            }

            return true;
#endif
        }

        public bool IsRegistered(string name)
        {
            foreach (var task in _scope.ResolveOptional<IEnumerable<InProcessTask>>())
            {
                if (task.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    var registration = BackgroundTaskRegistration.AllTasks.FirstOrDefault(t => t.Value.Name == task.Name).Value;
                    return registration != null;
                }
            }

            return false;
        }

        public bool ToggleTask(string name)
        {
            foreach (var task in _scope.ResolveOptional<IEnumerable<InProcessTask>>())
            {
                if (task.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return ToggleTask(task);
                }
            }

            return false;
        }

        private bool ToggleTask(InProcessTask task)
        {
            var taskRegistration = BackgroundTaskRegistration.AllTasks.FirstOrDefault(t => t.Value.Name == task.Name).Value;
            if (task.IsEnabled)
            {
                if (taskRegistration == null)
                {
                    // Register the task.
                    _log.Debug("Registering in-proc background task {BackgroundTaskName}...", task.Name);
                    var builder = new BackgroundTaskBuilder { Name = task.Name };
                    task.Register(builder);
                    builder.Register();
                    return true;
                }
            }
            else
            {
                if (taskRegistration != null)
                {
                    // Unregister the task.
                    _log.Debug("Unregistering in-proc background task {BackgroundTaskName}...", task.Name);
                    taskRegistration.Unregister(true);
                    return false;
                }
            }

            return false;
        }
    }
}

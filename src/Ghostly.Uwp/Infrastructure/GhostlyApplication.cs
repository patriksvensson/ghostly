using System.Diagnostics.CodeAnalysis;
using Autofac;
using Ghostly.Core;
using Serilog;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace Ghostly.Uwp.Infrastructure
{
    public abstract class GhostlyApplication : Application
    {
        public IContainer Container { get; private set; }
        public IActivationService Activator => Container.Resolve<IActivationService>();

        protected GhostlyApplication()
        {
            Container = Configure();

            // Hook up events
            Current.Suspending += OnSuspending;
            Current.Resuming += OnResuming;
            Current.UnhandledException += OnUnhandledException;
        }

        protected abstract void Configure(ContainerBuilder builder);

        protected sealed override async void OnActivated(IActivatedEventArgs args)
        {
            if (args is null)
            {
                throw new System.ArgumentNullException(nameof(args));
            }

            Log.Debug("Activation: Kind={Kind}, PreviousExecutionState={PreviousExecutionState}", args.Kind, args.PreviousExecutionState);
            await Activator.Activate(args);

            if (args.Kind == ActivationKind.StartupTask)
            {
                Log.Information("Starting (via startup task)...");
                OnStarted();
                Log.Information("Started.");
            }
        }

        protected sealed override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            Log.Debug("Background activation: Name={TaskName}, Id={TaskId}, SuspensionCount={TaskSuspensionCount}",
                args?.TaskInstance?.Task?.Name ?? "Unknown",
                args?.TaskInstance?.InstanceId.ToString() ?? "Unknown",
                args?.TaskInstance.SuspendedCount);

            try
            {
                GhostlyState.IsBackgroundActivated = true;
                var service = Container.Resolve<IActivationService>();
                await service.Activate(args);
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "An error occured when background activated.");
            }
            finally
            {
                GhostlyState.IsBackgroundActivated = false;
            }
        }

        protected sealed override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args is null)
            {
                throw new System.ArgumentNullException(nameof(args));
            }

            Log.Debug("Launch: Kind={Kind}, PrelaunchActivated={PrelaunchActivated}, PreviousExecutionState={PreviousExecutionState}",
                args.Kind, args.PrelaunchActivated, args.PreviousExecutionState);

            if (!args.PrelaunchActivated)
            {
                GhostlyState.InForeground = true;
                await Activator.Activate(args);

                if (args.Kind == ActivationKind.Launch && args.PreviousExecutionState != ApplicationExecutionState.Running)
                {
                    Log.Debug("Launching Ghostly...");
                    OnStarted();
                    Log.Information("Ghostly launched.");
                }
            }
        }

        protected virtual void OnStarted()
        {
        }

        protected virtual void OnSuspended()
        {
        }

        protected virtual void OnResumed()
        {
        }

        private void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            Log.Debug("Suspending Ghostly...");
            var deferral = e.SuspendingOperation.GetDeferral();
            GhostlyState.InForeground = false;
            OnSuspended();
            deferral.Complete();
            Log.Debug("Ghostly suspended.");
        }

        private void OnResuming(object sender, object e)
        {
            Log.Debug("Resuming Ghostly...");
            GhostlyState.IsBackgroundActivated = false;
            GhostlyState.InForeground = true;
            OnResumed();
            Log.Debug("Ghostly resumed.");
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                Log.Error(e.Exception, "An unhandled exception occured.");

                TelemetryService.Instance.TrackException(e.Exception);
                TelemetryService.Instance.Flush();
            }
        }

        private IContainer Configure()
        {
            var builder = new ContainerBuilder();
            Configure(builder);
            return builder.Build();
        }
    }
}

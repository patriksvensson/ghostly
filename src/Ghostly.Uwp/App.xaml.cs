using Autofac;
using Ghostly.Core;
using Ghostly.Core.Services;
using Ghostly.Domain.Messages;
using Ghostly.Uwp.Infrastructure;
using Ghostly.Uwp.Infrastructure.Pal;

namespace Ghostly.Uwp
{
    public sealed partial class App : GhostlyApplication
    {
        public App()
        {
            InitializeComponent();

            Platform.ThreadingModel = new UwpThreadingModel();
        }

        protected override void Configure(ContainerBuilder builder)
        {
            builder.RegisterModule<AppModule>();
        }

        protected override void OnStarted()
        {
            // Start the job orchestrator for the first time.
            var orchestrator = Container.Resolve<IJobOrchestrator>();
            orchestrator.Start();
        }

        protected override void OnSuspended()
        {
            // Stop the job orchestrator.
            var orchestrator = Container.Resolve<IJobOrchestrator>();
            orchestrator.Shutdown(true);

            // Flush telemetry.
            TelemetryService.Instance.Flush();
        }

        protected override void OnResumed()
        {
            var service = Container.Resolve<IMessageService>();
            service.PublishAsync(new RefreshApplication()).FireAndForgetSafeAsync();

            // Restart the job orchestrator.
            var orchestrator = Container.Resolve<IJobOrchestrator>();
            orchestrator.Start();
        }
    }
}

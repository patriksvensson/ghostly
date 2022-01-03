using System;
using System.Threading.Tasks;
using Autofac;
using Ghostly.Core;
using Ghostly.Core.Services;
using Ghostly.Domain.Messages;
using Ghostly.GitHub;
using Ghostly.Uwp.Infrastructure;
using Ghostly.Uwp.Infrastructure.Pal;
using Windows.UI.Popups;

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

        protected override async Task OnStarted()
        {
#if DEBUG
            if (!GitHubSecrets.Instance.IsValid())
            {
                var dialog = new MessageDialog(
                    "GitHub OAUTH credentials have not been configured.\n" +
                    "See README.md for more information.");

                dialog.Title = "Configuration error";
                dialog.Commands.Add(new UICommand("Continue") { Id = 0 });
                dialog.Commands.Add(new UICommand("Quit") { Id = 1 });
                dialog.DefaultCommandIndex = 1;
                dialog.CancelCommandIndex = 0;

                var result = await dialog.ShowAsync();
                if ((int)result.Id == 1)
                {
                    Exit();
                    return;
                }
            }
#endif

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

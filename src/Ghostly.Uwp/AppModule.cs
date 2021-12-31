using Autofac;
using Ghostly.Core;
using Ghostly.Diagnostics;
using Ghostly.GitHub;
using Ghostly.Uwp.Activation;
using Ghostly.Uwp.Infrastructure;
using Ghostly.Uwp.Infrastructure.Pal;
using Ghostly.Uwp.Tasks;
using Ghostly.Uwp.Views;
using Windows.Storage;

namespace Ghostly.Uwp
{
    public sealed class AppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Shell
            builder.RegisterShell<ShellView>();

            // Modules
            builder.RegisterModule<GhostlyModule>();
            builder.RegisterModule<GitHubModule>();

            // TODO: Clean this up
            var level = UwpLocalSettings.GetValue(s => s.GetLogLevel());
            var path = ApplicationData.Current.LocalFolder.Path;
            builder.RegisterModule(new LogModule(level, path));

            // Telemetry
            builder.RegisterInstance(TelemetryService.Instance).AsSelf().AsImplementedInterfaces().SingleInstance();

            // Services
            builder.RegisterType<ActivationService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<NavigationService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ViewLocator>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ViewModelLocator>().AsSelf().SingleInstance();
            builder.RegisterType<DialogService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ThemeService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<WhatsNewService>().AsSelf().SingleInstance();
            builder.RegisterType<LocalizationService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<CultureService>().AsImplementedInterfaces().SingleInstance();

            // Platform
            builder.RegisterType<UwpBadgeUpdater>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpFileDownloader>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpFileSystem>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpFolderLauncher>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpLocalSettings>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpMarketHelper>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpNetworkHelper>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpPackageService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpPasswordVault>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpResourceReader>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpStartupHelper>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpSystemService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpThemeHelper>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpToastNotifier>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UwpUriLauncher>().AsImplementedInterfaces().SingleInstance();

            // Activation handlers
            builder.RegisterActivationHandler<LaunchActivationHandler>();
            builder.RegisterActivationHandler<BackgroundActivationHandler>();
            builder.RegisterActivationHandler<GitHubActivationHandler>();
            builder.RegisterActivationHandler<ToastActivationHandler>();

            // Background services.
            builder.RegisterType<BackgroundTaskService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ExtendedExecutionService>().AsImplementedInterfaces().SingleInstance();

            // Background tasks
            builder.RegisterType<SynchronizationTask>().As<InProcessTask>().InstancePerDependency();
        }
    }
}

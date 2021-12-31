using Autofac;
using Ghostly.Core;
using Ghostly.Core.Mvvm;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Features.Accounts;
using Ghostly.Features.Activities;
using Ghostly.Features.Activities.Processors;
using Ghostly.Features.Categories;
using Ghostly.Features.Notifications;
using Ghostly.Features.Rules;
using Ghostly.Features.Synchronization;
using Ghostly.Jobs;
using Ghostly.Services;
using Ghostly.Startup;
using Ghostly.ViewModels;
using Ghostly.ViewModels.Dialogs;
using MediatR;

namespace Ghostly
{
    public sealed class GhostlyModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Startup tasks
            builder.RegisterType<StartupLogger>().AsImplementedInterfaces().SingleInstance();

            // Database
            builder.RegisterType<DatabaseInitializer>().AsImplementedInterfaces().SingleInstance();

            // View models
            builder.RegisterViewModel<AccountViewModel>();
            builder.RegisterViewModel<MainViewModel>();
            builder.RegisterViewModel<RuleViewModel>();
            builder.RegisterViewModel<SettingsViewModel>();
            builder.RegisterViewModel<ShellViewModel>();

            // Dialog view models
            builder.RegisterDialogViewModel<WhatsNewViewModel>();
            builder.RegisterDialogViewModel<SelectCategoryViewModel>();
            builder.RegisterDialogViewModel<CreateCategoryViewModel>();
            builder.RegisterDialogViewModel<ConfirmActionViewModel>();
            builder.RegisterDialogViewModel<CreateRuleViewModel>();
            builder.RegisterDialogViewModel<MessageBoxViewModel>();
            builder.RegisterDialogViewModel<SelectProfileViewModel>();
            builder.RegisterDialogViewModel<NewProfileViewModel>();

            // Services
            builder.RegisterType<AuthenticationService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<MessageService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<JobOrchestrator>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<SynchronizationService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<NotificationService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<TemplateService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UnreadService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<CategoryService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ShellResolver>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<InitializationService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<AccountProblemDetector>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<RuleService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ProfileService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ProfileService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<Clock>().AsImplementedInterfaces().SingleInstance();

            // Handlers
            RegisterMediatr(builder);
            builder.RegisterType<ChangeNotificationStateHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<MarkAsReadHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetNotificationHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetNotificationsHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetAccountHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetAccountsHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetWorkItemHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpdateAccountHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<DownSyncHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpSyncHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetCategoriesHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<CheckVisibilityHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<MoveNotificationHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpdateAccountStateHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetRulesHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<CreateRuleHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<ReorderRulesHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpdateRuleHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<DeleteRuleHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetRulesForCategory>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<ProcessCategoryRuleHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<CreateCategoryHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpdateCategoryHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<DeleteCategoryHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<ReorderCategoriesHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<MoveNotificationsToInboxHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<MarkAllAsReadHandler>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<ArchiveAllHandler>().AsImplementedInterfaces().InstancePerDependency();

            // Jobs
            builder.RegisterType<SynchronizationJob>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ActivityQueueJob>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ActivityProcessingJob>().AsImplementedInterfaces().SingleInstance();

            // Sync activities
            builder.RegisterType<ActivityHelper>().AsSelf().SingleInstance();
            builder.RegisterType<MarkAsReadPayloadProcessor>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DownloadFilePayloadProcessor>().AsImplementedInterfaces().SingleInstance();

            // Data
            builder.RegisterType<GhostlyContext>().InstancePerDependency();
            builder.RegisterType<GhostlyContextFactory>().As<IGhostlyContextFactory>().InstancePerDependency();
            builder.RegisterType<DatabaseLock>().AsImplementedInterfaces().SingleInstance();
        }

        private void RegisterMediatr(ContainerBuilder builder)
        {
            builder.RegisterType<Mediator>()
              .As<IMediator>()
              .InstancePerLifetimeScope();

            builder.Register<ServiceFactory>(context =>
            {
                var c = context.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });
        }
    }
}

using Autofac;
using Ghostly.Core;
using Ghostly.Domain;
using Ghostly.GitHub.Actions;
using Ghostly.GitHub.Features;
using Ghostly.GitHub.Octokit;
using Ghostly.GitHub.Synchronizers;
using Ghostly.GitHub.ViewModels;

namespace Ghostly.GitHub
{
    public sealed class GitHubModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Vendor
            builder.RegisterType<GitHubVendor>().AsSelf().As<IVendor>().SingleInstance();
            builder.RegisterType<GitHubProfileService>().AsSelf().As<IVendorProfiles>().SingleInstance();

            // View models
            builder.RegisterDialogViewModel<GitHubPreferencesViewModel>();

            // Services
            builder.RegisterType<GitHubAccountProvider>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<GitHubAuthenticator>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<GitHubSynchronizer>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<GitHubSynchronizationQueue>().AsImplementedInterfaces().AsSelf().SingleInstance();

            // Synchronizers
            builder.RegisterType<NotificationSynchronizer>().AsSelf().SingleInstance();

            // Actions
            builder.RegisterType<GetGitHubUser>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetGitHubWorkItem>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetGitHubNotification>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetGitHubNotifications>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetGitHubRepository>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetGitHubWorkItemInformation>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<GetRemoteGitHubNotification>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpdateGitHubNotification>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpdateGitHubWorkItem>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpdateGitHubWorkItemComments>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpdateGitHubWorkItemLabels>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpdateGitHubWorkItemAssignees>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpdateGitHubWorkItemMilestone>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpdateLocallyUnreadItems>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<DownloadGitHubAvatar>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpdateGitHubPullRequestReviews>().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<UpdateGitHubCommitComments>().AsImplementedInterfaces().InstancePerDependency();

            // Gateway
            builder.RegisterType<GitHubGatewayFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<GitHubGateway>().AsSelf().AsImplementedInterfaces().SingleInstance();
        }
    }
}

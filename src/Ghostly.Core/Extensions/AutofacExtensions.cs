using Autofac;
using Ghostly.Core.Mvvm;

namespace Ghostly.Core
{
    public static class AutofacExtensions
    {
        public static void RegisterShell<TView>(this ContainerBuilder builder)
            where TView : IView
        {
            builder.RegisterType<TView>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        public static void RegisterViewModel<TViewModel>(this ContainerBuilder builder)
            where TViewModel : IViewModel
        {
            builder.RegisterType<TViewModel>()
                .AsSelf()
                .AsImplementedInterfaces()
                .Named<IViewModel>(typeof(TViewModel).Name)
                .SingleInstance();
        }

        public static void RegisterDialogViewModel<TViewModel>(this ContainerBuilder builder)
            where TViewModel : IViewModel
        {
            builder.RegisterType<TViewModel>()
                .AsSelf()
                .AsImplementedInterfaces()
                .Named<IViewModel>(typeof(TViewModel).Name)
                .InstancePerDependency();
        }

        public static void RegisterActivationHandler<T>(this ContainerBuilder builder)
            where T : IActivationHandler
        {
            builder.RegisterType(typeof(T)).As<IActivationHandler>().SingleInstance();
        }
    }
}

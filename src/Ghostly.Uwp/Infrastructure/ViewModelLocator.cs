using System;
using Autofac;
using Ghostly.Core.Mvvm;
using Windows.UI.Xaml;

namespace Ghostly.Uwp.Infrastructure
{
    public sealed class ViewModelLocator : IViewModelLocator
    {
        private readonly ILifetimeScope _lifetime;

        public ViewModelLocator(ILifetimeScope lifetime)
        {
            _lifetime = lifetime;
        }

        public object GetViewModel(object view, out bool rebind)
        {
            if (view is null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (view is FrameworkElement element)
            {
                if (element?.DataContext != null)
                {
                    // Got the right view model already?
                    var currentViewName = view.GetType().Name;
                    var currentDataContextName = element.DataContext.GetType().Name;
                    if (string.Equals($"{currentViewName}Model", currentDataContextName, StringComparison.Ordinal))
                    {
                        rebind = false;
                        return element.DataContext;
                    }
                }
            }

            rebind = true;
            return _lifetime.ResolveNamed<IViewModel>($"{view.GetType().Name}Model");
        }
    }
}

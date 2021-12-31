using System;
using System.Threading.Tasks;
using Autofac;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Mvvm;
using Ghostly.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Ghostly.Uwp.Infrastructure
{
    [DependentOn(typeof(DatabaseInitializer))]
    public sealed class NavigationService : INavigationService, IInitializable
    {
        private readonly ILifetimeScope _scope;
        private readonly ViewLocator _viewLocator;
        private readonly ViewModelLocator _viewModelLocator;
        private readonly ITelemetry _telemetry;
        private IShellView _shell;
        private Frame _frame;

        public bool HasContent => _frame?.Content != null;
        public bool CanGoBack => _frame?.CanGoBack ?? false;

        public event NavigatedEventHandler Navigated = (s, e) => { };

        public NavigationService(
            ILifetimeScope scope,
            ViewLocator viewLocator,
            ViewModelLocator viewModelLocator,
            ITelemetry telemetry)
        {
            _scope = scope;
            _viewLocator = viewLocator;
            _viewModelLocator = viewModelLocator;
            _telemetry = telemetry;
        }

        public Task<bool> Initialize(bool background)
        {
            if (!background)
            {
                _shell = _scope.Resolve<IShellView>();
                _frame = _shell.GetFrame();
                _frame.Navigated += FrameNavigated;

                return Task.FromResult(true);
            }

            // We're not fully initialized yet.
            return Task.FromResult(false);
        }

        public Task<bool> GoBack()
        {
            return Navigate(() =>
            {
                if (CanGoBack)
                {
                    _frame?.GoBack();
                    return true;
                }

                return false;
            });
        }

        public Task<bool> Navigate<TViewModel>(object parameter = null)
            where TViewModel : IViewModel
        {
            // Track what the user actually uses.
            if (parameter is NavigationItemInvokedEventArgs args)
            {
                if (!args.Programatically)
                {
                    _telemetry.TrackPageView(args.Kind.ToString());
                }
            }

            return Navigate(() =>
            {
                var view = _viewLocator.Resolve(typeof(TViewModel));
                if (view == null)
                {
                    return false;
                }

                return _frame?.Navigate(view, parameter, null) ?? false;
            });
        }

        private void FrameNavigated(object sender, NavigationEventArgs e)
        {
            _shell.UpdateNavigation(e);
        }

        private async Task<bool> Navigate(Func<bool> func)
        {
            // Deactivate old view.
            if (_frame?.Content is FrameworkElement old_view)
            {
                var old_vm = old_view?.DataContext;
                if (old_vm != null)
                {
                    if (old_view?.DataContext is IDeactivatable deactivatable)
                    {
                        await deactivatable.Deactivate();
                    }
                }
            }

            // Navigate
            if (!func())
            {
                return false;
            }

            if (!(_frame?.Content is FrameworkElement new_view))
            {
                throw new InvalidOperationException();
            }

            var new_vm = _viewModelLocator.GetViewModel(new_view, out var rebind);
            if (rebind)
            {
                new_view.DataContext = new_vm;

                if (new_view is IInitializableView initializable)
                {
                    await initializable.InitializeView(new_vm);
                }

                if (new_vm is IViewModel viewmodel)
                {
                    await viewmodel.ViewInitialized();
                }
            }

            if (new_vm is IActivatable activatable)
            {
                await activatable.Activate();
            }

            // Return success.
            return true;
        }
    }
}

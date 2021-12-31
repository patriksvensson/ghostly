using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Mvvm;
using Ghostly.Core.Services;
using Ghostly.Domain.Messages;
using Ghostly.Services;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Ghostly.Uwp.Infrastructure
{
    public sealed class ActivationService : IActivationService
    {
        private readonly ILifetimeScope _scope;
        private readonly IInitializationService _initializer;
        private readonly ViewModelLocator _viewModelLocator;
        private readonly INavigationService _navigator;
        private readonly IThemeService _theme;
        private readonly IMessageService _messenger;
        private readonly WhatsNewService _whatsnew;
        private readonly IGhostlyLog _log;
        private readonly List<IActivationHandler> _handlers;

        public ActivationService(
            ILifetimeScope scope,
            IInitializationService initializer,
            ViewModelLocator viewModelLocator,
            INavigationService navigator,
            IThemeService theme,
            IMessageService messenger,
            WhatsNewService whatsnew,
            IGhostlyLog log,
            IEnumerable<IActivationHandler> handlers)
        {
            _handlers = new List<IActivationHandler>(handlers ?? Array.Empty<IActivationHandler>());
            _scope = scope;
            _initializer = initializer;
            _viewModelLocator = viewModelLocator;
            _navigator = navigator;
            _theme = theme;
            _messenger = messenger;
            _whatsnew = whatsnew;
            _log = log;
        }

        private bool _uiInitialized;

        public async Task Activate(object args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var background = args is BackgroundActivatedEventArgs;

            // Initialize services.
            await _initializer.Initialize(background);

            // First time?
            var launchedAtStartup = args is IActivatedEventArgs activatedArgs && activatedArgs.Kind == ActivationKind.StartupTask;
            var launchedFromToast = args is ToastNotificationActivatedEventArgs;

            if (!_uiInitialized && !background)
            {
                if (launchedAtStartup)
                {
                    _log.Debug("Ghostly was launched at startup.");
                }

                if (Window.Current.Content == null)
                {
                    // Find the shell.
                    var shell = _scope.Resolve<IShellView>();
                    if (shell == null)
                    {
                        throw new InvalidOperationException("Could not resolve shell.");
                    }

                    // Bind the view model for the shell.
                    var view = shell.GetHandle();
                    var vm = _viewModelLocator.GetViewModel(view, out var rebind);
                    if (rebind)
                    {
                        ((FrameworkElement)view).DataContext = vm;

                        if (view is IInitializableView initializeView)
                        {
                            await initializeView.InitializeView(vm);
                        }

                        if (vm is IViewModel viewModel)
                        {
                            await viewModel.ViewInitialized();
                        }
                    }

                    // Display the shell.
                    Window.Current.Content = view;

                    // Initialize navigation.
                    InitializeNavigation();
                }
            }

            // Launched first time?
            if (!_uiInitialized)
            {
                foreach (var startup in _scope.Resolve<IEnumerable<IStartup>>())
                {
                    await startup.Start(background);
                }
            }

            // Handle the activation event.
            _log.Debug("Handling activation of type {ActivatedEventType}", args.GetType().FullName);
            var handler = _handlers.FirstOrDefault(h => h.CanHandle(args));
            if (handler != null)
            {
                _log.Debug("Found activation handler of type {ActivationHandlerType}.", handler.GetType().FullName);
                await handler.Handle(args);

                // Notice others who's interested in this.
                await _messenger.PublishAsync(new ApplicationActivated(background));
            }
            else
            {
                _log.Warning("Could not find activation handler.");
            }

            if (!background)
            {
                if (Window.Current != null)
                {
                    _log.Debug("Activating window");
                    Window.Current.Activate();
                }

                if (!_uiInitialized)
                {
                    // Set the theme.
                    await _theme.InitializeTheme();

                    // Show startup dialogs.
                    await _whatsnew.Show();
                }
            }

            if (!_uiInitialized && !background)
            {
                _uiInitialized = true;
            }
        }

        private void InitializeNavigation()
        {
            var navigationManager = SystemNavigationManager.GetForCurrentView();
            if (navigationManager != null)
            {
                navigationManager.BackRequested += OnBackRequested;
            }
        }

        private async void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = await _navigator.GoBack();
        }
    }
}

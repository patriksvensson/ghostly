using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Ghostly.Core.Mvvm;
using Ghostly.Core.Services;
using Ghostly.Services;
using Windows.UI.Xaml.Controls;

namespace Ghostly.Uwp.Infrastructure
{
    public sealed class DialogService : IDialogService
    {
        private readonly ViewLocator _viewLocator;
        private readonly IThemeService _theme;
        private readonly ILifetimeScope _lifetime;

        public DialogService(
            ViewLocator viewLocator,
            IThemeService theme,
            ILifetimeScope lifetime)
        {
            _viewLocator = viewLocator;
            _theme = theme;
            _lifetime = lifetime;
        }

        public async Task<TResponse> ShowDialog<TResponse>(IDialogRequest<TResponse> request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();

            var type = typeof(IDialogViewModel<,>).MakeGenericType(requestType, typeof(TResponse));
            var viewModel = _lifetime.Resolve(type);
            var viewModelType = viewModel.GetType();

            // Initialize the view model.
            var initializeMethod = viewModelType.GetMethod("Initialize", new[] { requestType });
            Debug.Assert(initializeMethod != null, "Could not locate Initialize() method for dialog.");
            initializeMethod.Invoke(viewModel, new[] { request });

            // Locate the view.
            var viewType = _viewLocator.Resolve(viewModelType);
            if (viewType == null)
            {
                throw new InvalidOperationException($"Could not locate view for '{viewModelType.Name}'.");
            }

            // Instantiate the view.
            var view = Activator.CreateInstance(viewType) as ContentDialog;
            if (view == null)
            {
                throw new InvalidOperationException("Could not instantiate view.");
            }

            // Set the view's requested theme.
            view.RequestedTheme = _theme.Current.ToElementTheme();

            // Bind the context.
            view.DataContext = viewModel;

            // Activatable?
            if (viewModel is IActivatable activatable)
            {
                await activatable.Activate();
            }

            // Show the dialog.
            var result = await view.ShowAsync(ContentDialogPlacement.Popup);

            // Deactivatable?
            if (viewModel is IDeactivatable deactivatable)
            {
                await deactivatable.Deactivate();
            }

            // Return the result.
            var getResultsMethod = viewModelType.GetMethod("GetResult");
            Debug.Assert(getResultsMethod != null, "Could not locate GetResults() method for dialog.");
            return (TResponse)getResultsMethod.Invoke(viewModel, new object[] { result == ContentDialogResult.Primary });
        }
    }
}

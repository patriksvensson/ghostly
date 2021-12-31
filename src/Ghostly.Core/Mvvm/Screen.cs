using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Ghostly.Core.Mvvm
{
    public abstract class Screen : Observable, IActivatable, IDeactivatable, IViewModel
    {
        public bool IsInitialized { get; private set; }
        public bool IsActive { get; private set; }

        async Task IActivatable.Activate()
        {
            if (!IsInitialized)
            {
                await OnInitialize();
                IsInitialized = true;
            }

            IsActive = true;
            await OnActivate();
        }

        async Task IDeactivatable.Deactivate()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            await OnDeactivate();
        }

        Task IViewModel.ViewInitialized()
        {
            return OnViewInitialized();
        }

        protected virtual Task OnViewInitialized()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnInitialize()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnActivate()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnDeactivate()
        {
            return Task.CompletedTask;
        }
    }
}

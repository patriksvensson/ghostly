using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Ghostly.Core.Mvvm
{
    public abstract class DialogScreen<TRequest, TResponse> : Observable, IDialogViewModel<TRequest, TResponse>
         where TRequest : IDialogRequest<TResponse>
    {
        public abstract TResponse GetResult(bool ok);
        public abstract void Initialize(TRequest request);

        public virtual Task OnShown()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnViewInitialized()
        {
            return Task.CompletedTask;
        }

        Task IViewModel.ViewInitialized()
        {
            return OnViewInitialized();
        }
    }
}

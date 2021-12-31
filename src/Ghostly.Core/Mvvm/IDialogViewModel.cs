using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Ghostly.Core.Mvvm
{
    public interface IDialogViewModel
    {
        Task OnShown();
    }

    public interface IDialogViewModel<in TRequest, TResponse> : IViewModel, IDialogViewModel
        where TRequest : IDialogRequest<TResponse>
    {
        void Initialize(TRequest request);
        TResponse GetResult(bool ok);
    }

    [SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Marker interface")]
    public interface IDialogRequest<TResult>
    {
    }
}

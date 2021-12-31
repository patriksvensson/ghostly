using System.Threading.Tasks;
using Ghostly.Core.Mvvm;

namespace Ghostly.Core.Services
{
    public interface IDialogService
    {
        Task<TResponse> ShowDialog<TResponse>(IDialogRequest<TResponse> request);
    }
}

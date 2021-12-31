using System.Threading.Tasks;
using System.Windows.Input;

namespace Ghostly.Core.Mvvm.Commands
{
    public interface IAsyncCommand<T> : ICommand
    {
        Task ExecuteAsync(T parameter);
    }
}

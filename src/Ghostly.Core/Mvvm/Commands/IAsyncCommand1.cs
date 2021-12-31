using System.Threading.Tasks;
using System.Windows.Input;

namespace Ghostly.Core.Mvvm.Commands
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync();
        bool CanExecute();
    }
}

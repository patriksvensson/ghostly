using System.Threading.Tasks;

namespace Ghostly.Core.Mvvm
{
    public interface IInitializableView
    {
        Task InitializeView(object context);
    }
}

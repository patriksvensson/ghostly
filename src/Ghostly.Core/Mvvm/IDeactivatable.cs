using System.Threading.Tasks;

namespace Ghostly.Core.Mvvm
{
    public interface IDeactivatable
    {
        Task Deactivate();
    }
}

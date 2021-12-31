using System.Threading.Tasks;

namespace Ghostly.Core
{
    public interface IActivationService
    {
        Task Activate(object args);
    }
}

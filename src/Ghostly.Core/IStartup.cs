using System.Threading.Tasks;

namespace Ghostly.Core
{
    public interface IStartup
    {
        Task Start(bool background);
    }
}

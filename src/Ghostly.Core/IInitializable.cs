using System.Threading.Tasks;

namespace Ghostly.Core
{
    public interface IInitializable
    {
        Task<bool> Initialize(bool background);
    }
}

using System.Threading.Tasks;

namespace Ghostly.Core
{
    public interface IBackgroundInitializable
    {
        Task InitializeInBackground();
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace Ghostly.Core
{
    public interface IBackgroundJob
    {
        bool Enabled { get; }
        Task<bool> Run(CancellationToken token);
    }
}

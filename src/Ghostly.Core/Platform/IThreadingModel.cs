using System;
using System.Threading.Tasks;

namespace Ghostly.Core.Pal
{
    public interface IThreadingModel
    {
        Task ExecuteOnUIThread(Action action);
    }
}

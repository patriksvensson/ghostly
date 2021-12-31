using System;
using System.Threading.Tasks;
using Ghostly.Core.Pal;

namespace Ghostly.Core.Threading
{
    public sealed class DefaultThreadingModel : IThreadingModel
    {
        public Task ExecuteOnUIThread(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            action();
            return Task.CompletedTask;
        }
    }
}

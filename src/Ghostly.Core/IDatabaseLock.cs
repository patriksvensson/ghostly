using System;
using System.Threading.Tasks;

namespace Ghostly.Core
{
    public interface IDatabaseLock
    {
        IDisposable AcquireReadLock();
        Task<IDisposable> AcquireReadLockAsync();

        IDisposable AcquireWriteLock();
        Task<IDisposable> AcquireWriteLockAsync();
    }
}

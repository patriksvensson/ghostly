using System;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core;

namespace Ghostly.Data
{
    public sealed class DatabaseLock : IDatabaseLock, IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public DatabaseLock()
        {
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }

        private sealed class SemaphoreScope : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public SemaphoreScope(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                _semaphore.Release();
            }
        }

        public IDisposable AcquireReadLock()
        {
            _semaphore.Wait();
            return new SemaphoreScope(_semaphore);
        }

        public async Task<IDisposable> AcquireReadLockAsync()
        {
            await _semaphore.WaitAsync();
            return new SemaphoreScope(_semaphore);
        }

        public IDisposable AcquireWriteLock()
        {
            _semaphore.Wait();
            return new SemaphoreScope(_semaphore);
        }

        public async Task<IDisposable> AcquireWriteLockAsync()
        {
            await _semaphore.WaitAsync();
            return new SemaphoreScope(_semaphore);
        }
    }
}

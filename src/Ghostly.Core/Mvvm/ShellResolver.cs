using System;
using System.Threading;
using Autofac;

namespace Ghostly.Core.Mvvm
{
    /// <summary>
    /// This class is used to get the shell in places where
    /// we cannot get it with dependency injection.
    /// </summary>
    public sealed class ShellResolver : IShellResolver, IDisposable
    {
        private readonly ILifetimeScope _scope;
        private readonly SemaphoreSlim _lock;
        private IShell _shell;

        public ShellResolver(ILifetimeScope scope)
        {
            _scope = scope;
            _lock = new SemaphoreSlim(1, 1);
        }

        public void Dispose()
        {
            _lock.Dispose();
        }

        public IShell GetShell()
        {
            if (_shell == null)
            {
                try
                {
                    _lock.Wait();
                    if (_shell == null)
                    {
                        _shell = _scope.Resolve<IShell>();
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }

            return _shell;
        }
    }
}

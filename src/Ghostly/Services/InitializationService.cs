using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Collections;
using Ghostly.Core.Diagnostics;

namespace Ghostly.Services
{
    public interface IInitializationService
    {
        Task Initialize(bool background);
    }

    internal sealed class InitializationService : IInitializationService, IDisposable
    {
        private readonly IGhostlyLog _log;
        private readonly IEnumerable<IInitializable> _services;
        private readonly IEnumerable<IBackgroundInitializable> _backgroundServices;
        private readonly HashSet<Type> _initializedTypes;
        private readonly SemaphoreSlim _lock;
        private bool _initialized;
        private bool _backgroundInitialized;

        public InitializationService(
            IGhostlyLog log,
            IEnumerable<IInitializable> services,
            IEnumerable<IBackgroundInitializable> backgroundServices)
        {
            _log = log;
            _services = services;
            _backgroundServices = backgroundServices;
            _initializedTypes = new HashSet<Type>();
            _lock = new SemaphoreSlim(1, 1);
        }

        public void Dispose()
        {
            _lock?.Dispose();
        }

        public async Task Initialize(bool background)
        {
            if (_initialized)
            {
                return;
            }

            try
            {
                _log.Verbose("Waiting to aquire the initialization semaphore...");
                await _lock.WaitAsync();
                _log.Verbose("Aquired the initialization semaphore.");

                if (!_backgroundInitialized)
                {
                    foreach (var initializable in _backgroundServices)
                    {
                        initializable.InitializeInBackground().FireAndForgetSafe(
                            _log, $"An error occured during initialization of {initializable.GetType().Name}.");
                    }

                    _backgroundInitialized = true;
                }

                if (!_initialized)
                {
                    _log.Debug("Initializing services...");
                    var initializables = new DependencyCollection<IInitializable>(_services);
                    foreach (var initializable in initializables)
                    {
                        if (_initializedTypes.Contains(initializable.GetType()))
                        {
                            continue;
                        }

                        if (await initializable.Initialize(background))
                        {
                            _log.Debug("Initialized {InitializerServiceName}.", initializable.GetType().FullName);
                            _initializedTypes.Add(initializable.GetType());
                        }
                    }

                    _initialized = initializables.Count == _initializedTypes.Count;
                    if (_initialized)
                    {
                        _log.Debug("Services fully initialized.");
                    }
                    else
                    {
                        _log.Debug("Services partially initialized.");
                    }
                }
            }
            finally
            {
                _log.Verbose("Releasing the initialization semaphore...");
                _lock.Release();
                _log.Verbose("Released the initialization semaphore.");
            }
        }
    }
}

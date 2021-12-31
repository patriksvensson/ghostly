using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Threading;

namespace Ghostly.Core.Services
{
    public interface IJobOrchestrator
    {
        void Start();
        void Shutdown(bool join);
    }

    public sealed class JobOrchestrator : IJobOrchestrator, IDisposable
    {
        private readonly List<Task> _tasks;
        private readonly List<IBackgroundJob> _workers;
        private readonly ManualResetEvent _stopped;
        private readonly IGhostlyLog _log;

        private CancellationTokenSource _source;

        public JobOrchestrator(
            IEnumerable<IBackgroundJob> workers,
            IGhostlyLog log)
        {
            _workers = new List<IBackgroundJob>(workers);
            _tasks = new List<Task>();
            _stopped = new ManualResetEvent(true);
            _log = log;
        }

        public void Dispose()
        {
            _source.Dispose();
            _stopped.Dispose();
        }

        public void Start()
        {
            if (!_stopped.WaitOne(0))
            {
                return;
            }

            _log.Information("Starting background jobs...");

            _stopped.Reset();
            _source = new CancellationTokenSource();
            _tasks.Clear();

            // Start all tasks.
            foreach (var worker in _workers.Where(w => w.Enabled))
            {
                _log.Debug("Starting job {JobType}...", worker.GetType().Name);
                _tasks.Add(new TaskWrapper(worker).Start(_source));
            }

            // Configure the tasks.
            Task.WhenAll(_tasks).ContinueWith(task => _stopped.Set());
        }

        public void Shutdown(bool join)
        {
            _log.Information("Shutting down active background jobs...");

            if (_source != null)
            {
                if (!_source.IsCancellationRequested)
                {
                    _log.Debug("Telling jobs to abort...");
                    _source.Cancel();
                }
            }

            if (join)
            {
                _log.Debug("Waiting for jobs to abort...");
                _stopped.WaitOne();
                _log.Debug("All jobs aborted.");
            }
        }
    }
}
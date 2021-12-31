using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ghostly.Core.Threading
{
    public sealed class TaskWrapper
    {
        private readonly IBackgroundJob _worker;
        private CancellationTokenSource _source;

        public Task Task { get; private set; }

        public TaskWrapper(IBackgroundJob worker)
        {
            _worker = worker;
        }

        public Task Start(CancellationTokenSource source)
        {
            _source = source;

            Task = Task.Factory.StartNew(
                async () =>
                {
                    while (!_source.Token.WaitHandle.WaitOne(0))
                    {
                        try
                        {
                            // Run the task.
                            var result = await _worker.Run(_source.Token);
                            if (!result)
                            {
                                break;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // Aborted
                        }
                        catch (Exception)
                        {
                            // Error
                        }
                        finally
                        {
                            // Stopped
                        }
                    }
                }, TaskCreationOptions.LongRunning);

            return Task;
        }
    }
}

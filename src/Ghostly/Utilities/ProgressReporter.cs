using System;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Domain.Messages;

namespace Ghostly.Utilities
{
    public abstract class ProgressReporter : IDisposable
    {
        protected IMessageService Messenger { get; }

        private static AsyncLocal<int> _count;
        static ProgressReporter()
        {
            _count = new AsyncLocal<int>();
        }

        protected ProgressReporter(IMessageService messenger)
        {
            Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _count.Value += 1;
        }

        public abstract Task ShowProgress(string message);

        public async Task ShowProgress(bool condition, string message)
        {
            if (condition)
            {
                await ShowProgress(message);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _count.Value -= 1;

            // Last one?
            if (_count.Value == 0)
            {
                Messenger.Publish(new ShowProgress(string.Empty, null));
            }
        }
    }
}

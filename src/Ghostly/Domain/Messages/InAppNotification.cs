using System;
using SystemTimeout = System.Threading.Timeout;

namespace Ghostly.Domain.Messages
{
    public sealed class InAppNotification
    {
        public string Message { get; }
        public int Timeout { get; }

        public InAppNotification(string message)
            : this(message, SystemTimeout.InfiniteTimeSpan)
        {
        }

        public InAppNotification(string message, TimeSpan timeout)
        {
            Message = message;
            Timeout = timeout == SystemTimeout.InfiniteTimeSpan ? 0 : (int)timeout.TotalMilliseconds;
        }
    }
}

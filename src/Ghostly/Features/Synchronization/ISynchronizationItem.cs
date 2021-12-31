using System;

namespace Ghostly.Features.Synchronization
{
    public interface ISynchronizationItem
    {
        string Identity { get; }
        DateTime Timestamp { get; }
    }
}

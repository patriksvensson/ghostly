using System;
using System.Diagnostics;

namespace Ghostly.Features.Synchronization
{
    [DebuggerDisplay("{Identity,nq} {Timestamp}")]
    internal sealed class SynchronizationItem : ISynchronizationItem
    {
        public string Identity { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

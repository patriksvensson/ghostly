namespace Ghostly.Domain.Messages
{
    public sealed class SynchronizationStateChanged
    {
        public bool IsSynchronizing { get; }
        public bool IsSynchronizingSingle { get; }

        public SynchronizationStateChanged(bool isSynchronizing, bool isSynchronizingSingle)
        {
            IsSynchronizing = isSynchronizing;
            IsSynchronizingSingle = isSynchronizingSingle;
        }
    }
}

namespace Ghostly.Domain.Messages
{
    public sealed class SynchronizationAvailabilityChanged
    {
        public bool CanSynchronize { get; }

        public SynchronizationAvailabilityChanged(bool canSynchronize)
        {
            CanSynchronize = canSynchronize;
        }
    }
}

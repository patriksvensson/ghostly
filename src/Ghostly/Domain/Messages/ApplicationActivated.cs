namespace Ghostly.Domain.Messages
{
    public sealed class ApplicationActivated
    {
        public bool IsBackgroundActivated { get; }

        public ApplicationActivated(bool isBackgroundActivated)
        {
            IsBackgroundActivated = isBackgroundActivated;
        }
    }
}

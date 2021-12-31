namespace Ghostly.Domain.Messages
{
    public sealed class GhostlyDisabled
    {
        public bool Disabled { get; }

        public GhostlyDisabled(bool disabled)
        {
            Disabled = disabled;
        }
    }
}

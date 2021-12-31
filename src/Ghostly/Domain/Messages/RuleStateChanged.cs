namespace Ghostly.Domain.Messages
{
    public sealed class RuleStateChanged
    {
        public bool Enabled { get; }

        public RuleStateChanged(bool enabled)
        {
            Enabled = enabled;
        }
    }
}

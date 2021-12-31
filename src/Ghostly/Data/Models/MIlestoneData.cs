namespace Ghostly.Data.Models
{
    public sealed class MilestoneData : EntityData
    {
        public Discriminator Discriminator { get; set; }

        // Generic
        public string Name { get; set; }

        // GitHub specific
        public long? GitHubId { get; set; }
    }
}

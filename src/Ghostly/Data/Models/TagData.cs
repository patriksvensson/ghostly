namespace Ghostly.Data.Models
{
    public sealed class TagData : EntityData
    {
        public Discriminator Discriminator { get; set; }

        // Generic
        public string Name { get; set; }

        // GitHub specific
        public long? GitHubId { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
    }
}

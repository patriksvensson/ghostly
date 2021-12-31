namespace Ghostly.Data.Models
{
    public sealed class RepositoryData : EntityData
    {
        public Discriminator Discriminator { get; set; }

        // Generic
        public bool Private { get; set; }
        public bool Fork { get; set; }

        // GitHub specific
        public long? GitHubId { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }
    }
}

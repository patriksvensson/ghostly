namespace Ghostly.Data.Models
{
    public sealed class UserData : EntityData
    {
        public Discriminator Discriminator { get; set; }

        // GitHub specific
        public int? GitHubId { get; set; }
        public string Login { get; set; }
        public string AvatarUrl { get; set; }
        public string AvatarHash { get; set; }
        public string Name { get; set; }
    }
}

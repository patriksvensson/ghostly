namespace Ghostly.Domain
{
    public abstract class User : Entity
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public string AvatarHash { get; set; }
    }
}

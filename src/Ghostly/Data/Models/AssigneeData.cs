namespace Ghostly.Data.Models
{
    public sealed class AssigneeData : EntityData
    {
        public Discriminator Discriminator { get; set; }

        // GitHub specific
        public UserData User { get; set; }
    }
}

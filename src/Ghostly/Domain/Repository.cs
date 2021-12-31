namespace Ghostly.Domain
{
    public abstract class Repository : Entity
    {
        public bool Private { get; set; }
        public bool Fork { get; set; }
    }
}

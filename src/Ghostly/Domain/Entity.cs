using Ghostly.Core.Mvvm;

namespace Ghostly.Domain
{
    public abstract class Entity : Observable
    {
        public int Id { get; set; }
    }
}

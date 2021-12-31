using Ghostly.Data.Models;

namespace Ghostly.Features.Activities
{
    public abstract class ActivityPayload
    {
        public abstract ActivityCategory Category { get; }
        public abstract ActivityKind Kind { get; }
        public virtual ActivityConstraint Contstraint => ActivityConstraint.None;
    }
}

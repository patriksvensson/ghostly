using System.Collections.Generic;

namespace Ghostly.Features.Querying.Expressions
{
    public interface IQueryProperty
    {
        IReadOnlyList<string> Names { get; }
    }
}

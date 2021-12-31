using System.Collections.Generic;
using System.Linq;

namespace Ghostly
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            if (source != null)
            {
                foreach (var item in items ?? Enumerable.Empty<T>())
                {
                    source.Add(item);
                }
            }
        }
    }
}

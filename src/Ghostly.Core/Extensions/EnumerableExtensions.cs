using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ghostly
{
    public static class EnumerableExtensions
    {
        public static void Apply<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (var item in source)
            {
                action(item);
            }
        }

        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source)
        {
            return source as IReadOnlyList<T> ?? new List<T>(source ?? Enumerable.Empty<T>());
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int take)
        {
            return source.Select((item, index) => new { item, index })
                        .GroupBy(x => x.index / take)
                        .Select(g => g.Select(x => x.item));
        }

        public static int SafeCount<T>(this IEnumerable<T> source)
        {
            if (source != null)
            {
                if (source is ICollection collection)
                {
                    return collection.Count;
                }

                return source.Count();
            }

            return 0;
        }
    }
}

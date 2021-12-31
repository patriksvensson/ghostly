using System;
using System.Linq;
using System.Linq.Expressions;

namespace Ghostly.Core
{
    public static class QueryableExtensions
    {
        public static IQueryable<TSource> OptionalWhere<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if (predicate != null)
            {
                return source.Where(predicate);
            }

            return source;
        }
    }
}

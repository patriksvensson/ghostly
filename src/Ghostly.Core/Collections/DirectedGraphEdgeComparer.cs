using System.Collections.Generic;

namespace Ghostly.Core.Collections
{
    public sealed class DirectedGraphEdgeComparer<T> : IEqualityComparer<DirectedGraphEdge<T>>
    {
        private readonly IEqualityComparer<T> _comparer;

        public DirectedGraphEdgeComparer(IEqualityComparer<T> comparer)
        {
            _comparer = comparer;
        }

        public bool Equals(DirectedGraphEdge<T> x, DirectedGraphEdge<T> y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return _comparer.Equals(x.From, y.From)
                && _comparer.Equals(x.To, y.To);
        }

        public int GetHashCode(DirectedGraphEdge<T> obj)
        {
            if (obj is null)
            {
                throw new System.ArgumentNullException(nameof(obj));
            }

            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + _comparer.GetHashCode(obj.From);
                hash = (hash * 31) + _comparer.GetHashCode(obj.To);
                return hash;
            }
        }
    }
}
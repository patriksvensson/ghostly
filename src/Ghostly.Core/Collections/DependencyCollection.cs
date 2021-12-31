using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ghostly.Core.Collections
{
    public sealed class DependencyCollection<T> : IEnumerable<T>
    {
        private readonly DirectedGraph<T> _graph;

        public int Count => _graph.Nodes.Count;

        public DependencyCollection(IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            _graph = new DirectedGraph<T>(source ?? Enumerable.Empty<T>(), comparer);

            foreach (var node in _graph.Nodes)
            {
                var attributes = node.GetType().GetCustomAttributes<DependentOnAttribute>();
                foreach (var attribute in attributes)
                {
                    var dependency = _graph.Nodes.SingleOrDefault(
                        x => x.GetType() == attribute.DependentOn ||
                        attribute.DependentOn.IsAssignableFrom(x.GetType()));

                    if (dependency != null)
                    {
                        _graph.Connect(dependency, node);
                    }
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _graph.Traverse().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
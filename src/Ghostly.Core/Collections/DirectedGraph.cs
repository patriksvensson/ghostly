using System;
using System.Collections.Generic;
using System.Linq;

namespace Ghostly.Core.Collections
{
    public sealed class DirectedGraph<T>
    {
        private readonly IEqualityComparer<T> _comparer;
        private readonly HashSet<T> _nodes;
        private readonly HashSet<DirectedGraphEdge<T>> _edges;

        public ISet<T> Nodes => _nodes;
        public ISet<DirectedGraphEdge<T>> Edges => _edges;

        public DirectedGraph(IEnumerable<T> source = null, IEqualityComparer<T> comparer = null)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;

            _nodes = new HashSet<T>(source ?? Enumerable.Empty<T>(), _comparer);
            _edges = new HashSet<DirectedGraphEdge<T>>(new DirectedGraphEdgeComparer<T>(_comparer));
        }

        public void Add(T item)
        {
            _nodes.Add(item);
        }

        public void Connect(T from, T to)
        {
            if (_comparer.Equals(from, to))
            {
                throw new InvalidOperationException("Reflexive edges in graph are not allowed.");
            }

            if (_edges.Contains(new DirectedGraphEdge<T>(to, from)))
            {
                throw new InvalidOperationException("Unidirectional edges in graph are not allowed.");
            }

            if (!_nodes.Contains(from))
            {
                _nodes.Add(from);
            }

            if (!_nodes.Contains(to))
            {
                _nodes.Add(to);
            }

            _edges.Add(new DirectedGraphEdge<T>(from, to));
        }

        public IEnumerable<T> Traverse()
        {
            // Find target nodes.
            var targets = _nodes.Where(node => !_edges.Any(edge => _comparer.Equals(node, edge.From)));

            // Iterate each target.
            var result = new List<T>();
            var visited = new HashSet<T>(_comparer);
            foreach (var target in targets)
            {
                Traverse(target, result, visited);
            }

            return result;
        }

        private void Traverse(T node, ICollection<T> result, ISet<T> visited = null)
        {
            visited = visited ?? new HashSet<T>(_comparer);
            if (!visited.Contains(node))
            {
                visited.Add(node);
                var incoming = _edges.Where(x => _comparer.Equals(x.To, node)).Select(x => x.From);
                foreach (var child in incoming)
                {
                    Traverse(child, result, visited);
                }

                result.Add(node);
            }
            else if (!result.Any(x => _comparer.Equals(x, node)))
            {
                throw new InvalidOperationException("Graph contains circular references.");
            }
        }
    }
}
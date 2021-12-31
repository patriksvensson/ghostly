namespace Ghostly.Core.Collections
{
    public sealed class DirectedGraphEdge<T>
    {
        public T From { get; }
        public T To { get; }

        public DirectedGraphEdge(T from, T to)
        {
            From = from;
            To = to;
        }
    }
}
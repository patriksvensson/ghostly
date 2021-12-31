using System;

namespace Ghostly.Features.Querying.Expressions.Ast
{
    public sealed class CollectionExpression : GhostlyExpression
    {
        public string Name { get; }

        public CollectionExpression(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override TResult Accept<TContext, TResult>(IQueryVisitor<TContext, TResult> visitor, TContext context)
        {
            return visitor.Visit(this, context);
        }

        public override string ToString()
        {
            return $"Collection({Name})";
        }
    }
}

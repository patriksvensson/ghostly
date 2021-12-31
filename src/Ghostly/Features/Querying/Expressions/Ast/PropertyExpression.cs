using System;
using Ghostly.Features.Querying.Expressions.Parsing;
using Superpower.Model;

namespace Ghostly.Features.Querying.Expressions.Ast
{
    public sealed class PropertyExpression : GhostlyExpression
    {
        public string Name { get; }

        private PropertyExpression(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        internal static GhostlyExpression Create(Token<ExpressionToken> token)
        {
            var value = token.ToStringValue();
            return new PropertyExpression(value);
        }

        internal static GhostlyExpression Create(string name)
        {
            return new PropertyExpression(name);
        }

        public override TResult Accept<TContext, TResult>(IQueryVisitor<TContext, TResult> visitor, TContext context)
        {
            return visitor.Visit(this, context);
        }

        public override string ToString()
        {
            return $"Property({Name})";
        }
    }
}

using System;

namespace Ghostly.Features.Querying.Expressions.Ast
{
    public sealed class ConstantExpression : GhostlyExpression
    {
        public Type Type { get; }
        public object Value { get; }

        public ConstantExpression(Type type, object value)
        {
            Type = type;
            Value = value;
        }

        public static ConstantExpression Create<T>(T value)
        {
            return new ConstantExpression(typeof(T), value);
        }

        public override TResult Accept<TContext, TResult>(IQueryVisitor<TContext, TResult> visitor, TContext context)
        {
            return visitor.Visit(this, context);
        }

        public override string ToString()
        {
            var value = Value is string ? $"'{Value}'" : Value;
            return $"Constant({value})";
        }
    }
}

namespace Ghostly.Features.Querying.Expressions.Ast
{
    public sealed class StringLiteralExpression : GhostlyExpression
    {
        public string Value { get; }

        public StringLiteralExpression(string value)
        {
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
            return $"StringLiteral({value})";
        }
    }
}

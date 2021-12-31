namespace Ghostly.Features.Querying.Expressions.Ast
{
    public sealed class ComparisonExpression : BinaryExpression
    {
        public ComparisonOperator Operator { get; }

        public ComparisonExpression(ComparisonOperator op, GhostlyExpression left, GhostlyExpression right)
            : base(left, right)
        {
            Operator = op;
        }

        internal static GhostlyExpression Create(ComparisonOperator @operator, GhostlyExpression left, GhostlyExpression right)
        {
            return new ComparisonExpression(@operator, left, right);
        }

        public override TResult Accept<TContext, TResult>(IQueryVisitor<TContext, TResult> visitor, TContext context)
        {
            return visitor.Visit(this, context);
        }

        public override string ToString()
        {
            return $"{Operator}({Left},{Right})";
        }
    }
}

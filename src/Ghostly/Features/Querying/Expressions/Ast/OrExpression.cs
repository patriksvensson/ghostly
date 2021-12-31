namespace Ghostly.Features.Querying.Expressions.Ast
{
    public sealed class OrExpression : ConnectiveExpression
    {
        public OrExpression(GhostlyExpression left, GhostlyExpression right)
            : base(left, right)
        {
        }

        public override TResult Accept<TContext, TResult>(IQueryVisitor<TContext, TResult> visitor, TContext context)
        {
            return visitor.Visit(this, context);
        }

        public override string ToString()
        {
            return $"Or({Left},{Right})";
        }
    }
}

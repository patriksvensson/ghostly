namespace Ghostly.Features.Querying.Expressions.Ast
{
    public sealed class NotExpression : UnaryExpression
    {
        public NotExpression(GhostlyExpression operand)
            : base(operand)
        {
        }

        public override TResult Accept<TContext, TResult>(IQueryVisitor<TContext, TResult> visitor, TContext context)
        {
            return visitor.Visit(this, context);
        }

        public override string ToString()
        {
            return $"Not({Operand})";
        }
    }
}

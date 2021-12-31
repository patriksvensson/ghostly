namespace Ghostly.Features.Querying.Expressions
{
    public abstract class GhostlyExpression
    {
        public abstract TResult Accept<TContext, TResult>(IQueryVisitor<TContext, TResult> visitor, TContext context);
    }

    public static class GhostlyExpressionExtensions
    {
        public static TResult Accept<TResult>(this GhostlyExpression expression, IQueryVisitor<object, TResult> visitor)
        {
            if (expression is null)
            {
                throw new System.ArgumentNullException(nameof(expression));
            }

            return expression.Accept(visitor, null);
        }
    }
}

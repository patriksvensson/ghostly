using Ghostly.Features.Querying.Expressions.Ast;

namespace Ghostly.Features.Querying.Expressions
{
    public interface IQueryVisitor<in TContext, out TResult>
    {
        TResult Visit(AndExpression expression, TContext context);
        TResult Visit(ComparisonExpression expression, TContext context);
        TResult Visit(ConstantExpression expression, TContext context);
        TResult Visit(NotExpression expression, TContext context);
        TResult Visit(OrExpression expression, TContext context);
        TResult Visit(CollectionExpression expression, TContext context);
        TResult Visit(PropertyExpression expression, TContext context);
        TResult Visit(StringLiteralExpression expression, TContext context);
    }
}

using Ghostly.Features.Querying.Expressions.Ast;

namespace Ghostly.Features.Querying.Expressions
{
    public abstract class QueryVisitor<TContext> : IQueryVisitor<TContext, object>
    {
        protected abstract void Visit(AndExpression expression, TContext context);
        protected abstract void Visit(ComparisonExpression expression, TContext context);
        protected abstract void Visit(ConstantExpression expression, TContext context);
        protected abstract void Visit(NotExpression expression, TContext context);
        protected abstract void Visit(OrExpression expression, TContext context);
        protected abstract void Visit(CollectionExpression expression, TContext context);
        protected abstract void Visit(PropertyExpression expression, TContext context);
        protected abstract void Visit(StringLiteralExpression expression, TContext context);

        object IQueryVisitor<TContext, object>.Visit(AndExpression expression, TContext context)
        {
            Visit(expression, context);
            return null;
        }

        object IQueryVisitor<TContext, object>.Visit(ComparisonExpression expression, TContext context)
        {
            Visit(expression, context);
            return null;
        }

        object IQueryVisitor<TContext, object>.Visit(ConstantExpression expression, TContext context)
        {
            Visit(expression, context);
            return null;
        }

        object IQueryVisitor<TContext, object>.Visit(NotExpression expression, TContext context)
        {
            Visit(expression, context);
            return null;
        }

        object IQueryVisitor<TContext, object>.Visit(OrExpression expression, TContext context)
        {
            Visit(expression, context);
            return null;
        }

        object IQueryVisitor<TContext, object>.Visit(CollectionExpression expression, TContext context)
        {
            Visit(expression, context);
            return null;
        }

        object IQueryVisitor<TContext, object>.Visit(PropertyExpression expression, TContext context)
        {
            Visit(expression, context);
            return null;
        }

        object IQueryVisitor<TContext, object>.Visit(StringLiteralExpression expression, TContext context)
        {
            Visit(expression, context);
            return null;
        }
    }

    public abstract class QueryVisitor<TContext, TResult> : IQueryVisitor<TContext, TResult>
    {
        protected abstract TResult Visit(AndExpression expression, TContext context);
        protected abstract TResult Visit(ComparisonExpression expression, TContext context);
        protected abstract TResult Visit(ConstantExpression expression, TContext context);
        protected abstract TResult Visit(NotExpression expression, TContext context);
        protected abstract TResult Visit(OrExpression expression, TContext context);
        protected abstract TResult Visit(CollectionExpression expression, TContext context);
        protected abstract TResult Visit(PropertyExpression expression, TContext context);
        protected abstract TResult Visit(StringLiteralExpression expression, TContext context);

        TResult IQueryVisitor<TContext, TResult>.Visit(AndExpression expression, TContext context)
        {
            return Visit(expression, context);
        }

        TResult IQueryVisitor<TContext, TResult>.Visit(ComparisonExpression expression, TContext context)
        {
            return Visit(expression, context);
        }

        TResult IQueryVisitor<TContext, TResult>.Visit(ConstantExpression expression, TContext context)
        {
            return Visit(expression, context);
        }

        TResult IQueryVisitor<TContext, TResult>.Visit(NotExpression expression, TContext context)
        {
            return Visit(expression, context);
        }

        TResult IQueryVisitor<TContext, TResult>.Visit(OrExpression expression, TContext context)
        {
            return Visit(expression, context);
        }

        TResult IQueryVisitor<TContext, TResult>.Visit(CollectionExpression expression, TContext context)
        {
            return Visit(expression, context);
        }

        TResult IQueryVisitor<TContext, TResult>.Visit(PropertyExpression expression, TContext context)
        {
            return Visit(expression, context);
        }

        TResult IQueryVisitor<TContext, TResult>.Visit(StringLiteralExpression expression, TContext context)
        {
            return Visit(expression, context);
        }
    }
}

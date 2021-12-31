using System;
using Ghostly.Features.Querying.Expressions.Ast;

namespace Ghostly.Features.Querying.Expressions
{
    public abstract class QueryRewriter<TContext> : QueryVisitor<TContext, GhostlyExpression>
    {
        protected override GhostlyExpression Visit(AndExpression expression, TContext context)
        {
            return RewriteBinary(context, expression, (left, right) =>
                new AndExpression(left, right));
        }

        protected override GhostlyExpression Visit(ComparisonExpression expression, TContext context)
        {
            return RewriteBinary(context, expression, (left, right) =>
                new ComparisonExpression(expression.Operator, left, right));
        }

        protected override GhostlyExpression Visit(ConstantExpression expression, TContext context)
        {
            return expression;
        }

        protected override GhostlyExpression Visit(NotExpression expression, TContext context)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            return RewriteUnary(context, expression, child =>
                new NotExpression(child));
        }

        protected override GhostlyExpression Visit(OrExpression expression, TContext context)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            return RewriteBinary(context, expression, (left, right) =>
                new OrExpression(left, right));
        }

        protected override GhostlyExpression Visit(CollectionExpression expression, TContext context)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            return expression;
        }

        protected override GhostlyExpression Visit(PropertyExpression expression, TContext context)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            return expression;
        }

        protected override GhostlyExpression Visit(StringLiteralExpression expression, TContext context)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            return expression;
        }

        private GhostlyExpression RewriteUnary(TContext context, UnaryExpression expression, Func<GhostlyExpression, GhostlyExpression> factory)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var child = expression.Operand.Accept(this, context);
            if (!ReferenceEquals(child, expression.Operand))
            {
                return factory(child);
            }

            return expression;
        }

        private GhostlyExpression RewriteBinary(TContext context, BinaryExpression expression, Func<GhostlyExpression, GhostlyExpression, GhostlyExpression> factory)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var left = expression.Left.Accept(this, context);
            var right = expression.Right.Accept(this, context);

            if (!ReferenceEquals(left, expression.Left) ||
                !ReferenceEquals(right, expression.Right))
            {
                return factory(left, right);
            }

            return expression;
        }
    }
}

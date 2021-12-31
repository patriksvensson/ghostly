using System;
using Ghostly.Features.Querying.Expressions;
using Ghostly.Features.Querying.Expressions.Ast;

namespace Ghostly.Features.Querying.Rewriting
{
    /// <summary>
    /// Responsible for rewriting expressions like "@prop OR @other"
    /// to a more correct expression such as "@prop = true OR @other = true".
    /// </summary>
    internal sealed class ImplicitBooleanFixer : QueryRewriter<object>
    {
        public static ImplicitBooleanFixer Instance { get; } = new ImplicitBooleanFixer();

        protected override GhostlyExpression Visit(OrExpression expression, object context)
        {
            return RewriteBinary(expression, context,
                (left, right) => new OrExpression(left, right));
        }

        protected override GhostlyExpression Visit(AndExpression expression, object context)
        {
            return RewriteBinary(expression, context,
                (left, right) => new AndExpression(left, right));
        }

        protected override GhostlyExpression Visit(NotExpression expression, object context)
        {
            return RewriteUnary(expression, context,
                operand => new NotExpression(operand));
        }

        private GhostlyExpression RewriteBinary(BinaryExpression expression, object context, Func<GhostlyExpression, GhostlyExpression, GhostlyExpression> func)
        {
            var left = expression.Left.Accept(this, context);
            var right = expression.Right.Accept(this, context);

            if (left is PropertyExpression)
            {
                left = new ComparisonExpression(ComparisonOperator.Equals, left, new ConstantExpression(typeof(bool), true));
            }

            if (right is PropertyExpression)
            {
                right = new ComparisonExpression(ComparisonOperator.Equals, right, new ConstantExpression(typeof(bool), true));
            }

            return func(left, right);
        }

        private GhostlyExpression RewriteUnary(UnaryExpression expression, object context, Func<GhostlyExpression, GhostlyExpression> func)
        {
            var operand = expression.Operand.Accept(this, context);
            if (operand is PropertyExpression)
            {
                operand = new ComparisonExpression(ComparisonOperator.Equals, operand, new ConstantExpression(typeof(bool), true));
            }

            return func(operand);
        }
    }
}

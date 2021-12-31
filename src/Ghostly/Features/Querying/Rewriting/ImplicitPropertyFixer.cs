using System;
using Ghostly.Features.Querying.Expressions;
using Ghostly.Features.Querying.Expressions.Ast;

namespace Ghostly.Features.Querying.Rewriting
{
    internal sealed class ImplicitPropertyFixer : QueryRewriter<object>
    {
        public static ImplicitPropertyFixer Instance { get; } = new ImplicitPropertyFixer();

        protected override GhostlyExpression Visit(ComparisonExpression expression, object context)
        {
            // Only rewrite left side of a comparison.
            var left = expression.Left.Accept(this, context);
            if (left is ConstantExpression constant && constant.Value is string constantValue)
            {
                left = PropertyExpression.Create(constantValue);
            }

            return new ComparisonExpression(expression.Operator, left, expression.Right);
        }

        protected override GhostlyExpression Visit(AndExpression expression, object context)
        {
            return RewriteBinary(expression, context, (left, right) => new AndExpression(left, right));
        }

        protected override GhostlyExpression Visit(OrExpression expression, object context)
        {
            return RewriteBinary(expression, context, (left, right) => new OrExpression(left, right));
        }

        protected override GhostlyExpression Visit(NotExpression expression, object context)
        {
            return RewriteUnary(expression, context, operand => new NotExpression(operand));
        }

        private GhostlyExpression RewriteUnary(UnaryExpression expression, object context, Func<GhostlyExpression, GhostlyExpression> func)
        {
            var operand = expression.Operand.Accept(this, context);
            if (operand is ConstantExpression constant && constant.Value is string constantValue)
            {
                operand = PropertyExpression.Create(constantValue);
            }

            return func(operand);
        }

        private GhostlyExpression RewriteBinary(BinaryExpression expression, object context, Func<GhostlyExpression, GhostlyExpression, GhostlyExpression> func)
        {
            var left = expression.Left.Accept(this, context);
            var right = expression.Right.Accept(this, context);

            if (left is ConstantExpression constant && constant.Value is string constantValue)
            {
                left = PropertyExpression.Create(constantValue);
            }

            if (right is ConstantExpression rightConstant && rightConstant.Value is string rightConstantValue)
            {
                right = PropertyExpression.Create(rightConstantValue);
            }

            return func(left, right);
        }
    }
}

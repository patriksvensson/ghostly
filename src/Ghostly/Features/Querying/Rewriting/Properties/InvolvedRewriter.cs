using System;
using Ghostly.Features.Querying.Expressions;
using Ghostly.Features.Querying.Expressions.Ast;

namespace Ghostly.Features.Querying.Rewriting.Properties
{
    public sealed class InvolvedRewriter : QueryRewriter<object>
    {
        public static InvolvedRewriter Instance { get; } = new InvolvedRewriter();

        protected override GhostlyExpression Visit(ComparisonExpression expression, object context)
        {
            if (expression.Left is PropertyExpression property)
            {
                if (property.Name.Equals("involved", StringComparison.OrdinalIgnoreCase) ||
                    property.Name.Equals("involves", StringComparison.OrdinalIgnoreCase))
                {
                    var value = (expression.Right as ConstantExpression)?.Value as string;
                    value = value?.Trim()?.TrimStart('@');
                    if (string.IsNullOrWhiteSpace(value?.Trim()))
                    {
                        throw new GhostlyQueryLanguageException("The 'involved' property can only be used with a valid username.");
                    }

                    var left = new OrExpression(
                        new ComparisonExpression(
                            expression.Operator,
                            PropertyExpression.Create("author"),
                            ConstantExpression.Create(value)),
                        new ComparisonExpression(
                            expression.Operator,
                            PropertyExpression.Create("assigned"),
                            ConstantExpression.Create(value)));

                    var right = new OrExpression(
                        new ComparisonExpression(
                            expression.Operator,
                            PropertyExpression.Create("mentions"),
                            ConstantExpression.Create(value)),
                        new ComparisonExpression(
                            expression.Operator,
                            PropertyExpression.Create("commenter"),
                            ConstantExpression.Create(value)));

                    return new OrExpression(left, right);
                }
            }

            return base.Visit(expression, context);
        }
    }
}

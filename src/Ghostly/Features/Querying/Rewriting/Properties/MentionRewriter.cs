using System;
using Ghostly.Features.Querying.Expressions;
using Ghostly.Features.Querying.Expressions.Ast;

namespace Ghostly.Features.Querying.Rewriting.Properties
{
    public sealed class MentionRewriter : QueryRewriter<object>
    {
        public static MentionRewriter Instance { get; } = new MentionRewriter();

        protected override GhostlyExpression Visit(ComparisonExpression expression, object context)
        {
            if (expression.Left is PropertyExpression property)
            {
                if (property.Name.Equals("mention", StringComparison.OrdinalIgnoreCase) ||
                    property.Name.Equals("mentions", StringComparison.OrdinalIgnoreCase))
                {
                    var value = (expression.Right as ConstantExpression)?.Value as string;
                    if (string.IsNullOrWhiteSpace(value?.Trim()))
                    {
                        throw new GhostlyQueryLanguageException("The 'mention' property can only be used with a valid username.");
                    }

                    return new OrExpression(
                        new ComparisonExpression(
                            ComparisonOperator.Contains,
                            PropertyExpression.Create("body"),
                            new ConstantExpression(typeof(string), "@" + value.Trim().TrimStart('@'))),
                        new ComparisonExpression(
                            ComparisonOperator.Contains,
                            PropertyExpression.Create("comment"),
                            new ConstantExpression(typeof(string), "@" + value.Trim().TrimStart('@'))));
                }
            }

            return base.Visit(expression, context);
        }
    }
}

using System;
using Ghostly.Features.Querying.Expressions;
using Ghostly.Features.Querying.Expressions.Ast;

namespace Ghostly.Features.Querying.Rewriting
{
    internal sealed class PropertySimplifier : QueryRewriter<object>
    {
        public static PropertySimplifier Instance { get; } = new PropertySimplifier();

        protected override GhostlyExpression Visit(ComparisonExpression expression, object context)
        {
            if (expression.Operator != ComparisonOperator.Contains)
            {
                return expression;
            }

            if (expression.Left is PropertyExpression property)
            {
                if (property.Name.Equals("is", StringComparison.OrdinalIgnoreCase))
                {
                    if (expression.Right is ConstantExpression constant
                        && constant.Value is string constantValue)
                    {
                        return PropertyExpression.Create(constantValue);
                    }
                    else
                    {
                        throw new GhostlyQueryLanguageException(
                            $"The shorthand 'is' can only be used with a boolean property.");
                    }
                }
                else if (property.Name.Equals("state", StringComparison.OrdinalIgnoreCase))
                {
                    if (expression.Right is ConstantExpression constant
                        && constant.Value is string constantValue)
                    {
                        if (constantValue.Equals("open", StringComparison.OrdinalIgnoreCase))
                        {
                            return PropertyExpression.Create("open");
                        }
                        else if (constantValue.Equals("closed", StringComparison.OrdinalIgnoreCase))
                        {
                            return PropertyExpression.Create("close");
                        }
                        else if (constantValue.Equals("reopened", StringComparison.OrdinalIgnoreCase))
                        {
                            return PropertyExpression.Create("reopened");
                        }
                        else
                        {
                            throw new GhostlyQueryLanguageException(
                                $"The state '{constantValue}' is invalid. Valid states are 'open', 'closed', and 'reopened'");
                        }
                    }
                    else
                    {
                        throw new GhostlyQueryLanguageException(
                            "The 'state' shorthand can only be used with a valid state.");
                    }
                }
                else if (property.Name.Equals("has", StringComparison.OrdinalIgnoreCase))
                {
                    if (expression.Right is ConstantExpression constant
                        && constant.Value is string constantValue)
                    {
                        if (constantValue.Equals("review", StringComparison.OrdinalIgnoreCase) ||
                            constantValue.Equals("reviews", StringComparison.OrdinalIgnoreCase))
                        {
                            return new ComparisonExpression(
                                ComparisonOperator.GreaterThan,
                                PropertyExpression.Create("review_count"),
                                ConstantExpression.Create(0));
                        }
                        else
                        {
                            throw new GhostlyQueryLanguageException(
                                $"Unknown value '{constantValue}' for shorthand 'has'.");
                        }
                    }
                    else
                    {
                        throw new GhostlyQueryLanguageException(
                            $"The shorthand 'has' can only be used with a string.");
                    }
                }
            }

            return base.Visit(expression, context);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ghostly.Core.Services;
using Ghostly.Data.Models;
using Ghostly.Features.Querying.Expressions;
using Ghostly.Features.Querying.Expressions.Ast;

namespace Ghostly.Features.Rules
{
    public static class RuleDescriber
    {
        public static string Describe(
            GhostlyExpression expression,
            RuleData rule,
            ILocalizer localizer)
        {
            if (expression is null)
            {
                throw new System.ArgumentNullException(nameof(expression));
            }

            if (rule is null)
            {
                throw new System.ArgumentNullException(nameof(rule));
            }

            if (localizer is null)
            {
                throw new System.ArgumentNullException(nameof(localizer));
            }

            var context = new VisitorContext
            {
                Fragments = new List<string>(),
                Localizer = localizer,
            };

            expression.Accept(new Visitor(), context);

            var actions = GetConsequences(rule, localizer);

            var builder = new StringBuilder();
            builder.Append(string.Concat(localizer["Rules_Description_If"], " "));
            builder.Append(string.Join(" ", context.Fragments));
            builder.Append(string.Concat(" ", localizer["Rules_Description_Then"].ToLowerInvariant(), " "));
            builder.Append(actions);
            builder.Append('.');

            if (rule.StopProcessing)
            {
                builder.Append(string.Concat(" ", localizer["Rules_Description_StopsProcessing"]));
            }

            return builder.ToString();
        }

        private static string GetConsequences(RuleData rule, ILocalizer localizer)
        {
            var actions = new List<string>();
            if (rule.Star)
            {
                actions.Add(localizer["Rules_Description_StarIt"].ToLowerInvariant());
            }

            if (rule.Mute)
            {
                actions.Add(localizer["Rules_Description_MuteIt"].ToLowerInvariant());
            }

            if (rule.MarkAsRead)
            {
                actions.Add(localizer["Rules_Description_MarkItAsRead"].ToLowerInvariant());
            }

            if (rule.Category != null)
            {
                actions.Add(localizer.Format(localizer["Rules_Description_MoveItTo"].ToLowerInvariant(), rule.Category.Name));
            }

            if (actions.Count == 0)
            {
                return localizer["Rules_Description_DoNothing"].ToLowerInvariant();
            }
            else if (actions.Count > 1)
            {
                var last = actions.Last();
                actions.RemoveAt(actions.Count - 1);
                return string.Join(", ", actions) + " " + localizer["Rules_Description_And"].ToLowerInvariant() + " " + last;
            }
            else
            {
                return string.Join(", ", actions);
            }
        }

        internal sealed class VisitorContext
        {
            public ILocalizer Localizer { get; set; }
            public List<string> Fragments { get; set; }
        }

        private sealed class Visitor : QueryVisitor<VisitorContext>
        {
            protected override void Visit(AndExpression expression, VisitorContext context)
            {
                expression.Left.Accept(this, context);
                context.Fragments.Add(context.Localizer["Rules_Description_And"].ToLowerInvariant());
                expression.Right.Accept(this, context);
            }

            protected override void Visit(ComparisonExpression expression, VisitorContext context)
            {
                expression.Left.Accept(this, context);

                switch (expression.Operator)
                {
                    case ComparisonOperator.LessThan:
                        context.Fragments.Add(context.Localizer["Rules_Description_IsLessThan"].ToLowerInvariant());
                        break;
                    case ComparisonOperator.LessThanOrEquals:
                        context.Fragments.Add(context.Localizer["Rules_Description_IsLessThanOrEqualTo"].ToLowerInvariant());
                        break;
                    case ComparisonOperator.GreaterThan:
                        context.Fragments.Add(context.Localizer["Rules_Description_IsGreaterThan"].ToLowerInvariant());
                        break;
                    case ComparisonOperator.GreaterThanOrEquals:
                        context.Fragments.Add(context.Localizer["Rules_Description_IsGreaterThanOrEqualTo"].ToLowerInvariant());
                        break;
                    case ComparisonOperator.Equals:
                        context.Fragments.Add(context.Localizer["Rules_Description_Is"].ToLowerInvariant());
                        break;
                    case ComparisonOperator.NotEquals:
                        context.Fragments.Add(context.Localizer["Rules_Description_IsNot"].ToLowerInvariant());
                        break;
                    case ComparisonOperator.Contains:
                        context.Fragments.Add(context.Localizer["Rules_Description_Contains"].ToLowerInvariant());
                        break;
                }

                expression.Right.Accept(this, context);
            }

            protected override void Visit(ConstantExpression expression, VisitorContext context)
            {
                if (expression.Value is bool boolean)
                {
                    context.Fragments.Add((boolean ? context.Localizer["Rules_Description_True"] : context.Localizer["Rules_Description_False"]).ToLowerInvariant());
                }
                else if (expression.Value is string text)
                {
                    context.Fragments.Add($"\"{text}\"");
                }
                else
                {
                    context.Fragments.Add(expression.Value?.ToString() ?? context.Localizer["Rules_Description_Null"].ToLowerInvariant());
                }
            }

            protected override void Visit(NotExpression expression, VisitorContext context)
            {
                context.Fragments.Add(context.Localizer["Rules_Description_IsNot"].ToLowerInvariant());
                expression.Operand.Accept(this, context);
            }

            protected override void Visit(OrExpression expression, VisitorContext context)
            {
                expression.Left.Accept(this, context);
                context.Fragments.Add(context.Localizer["Rules_Description_Or"].ToLowerInvariant());
                expression.Right.Accept(this, context);
            }

            protected override void Visit(CollectionExpression expression, VisitorContext context)
            {
                context.Fragments.Add(expression.Name);
            }

            protected override void Visit(PropertyExpression expression, VisitorContext context)
            {
                context.Fragments.Add(expression.Name);
            }

            protected override void Visit(StringLiteralExpression expression, VisitorContext context)
            {
                context.Fragments.Add($"\"{expression.Value}\"");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Ghostly.Data.Models;
using Ghostly.Features.Querying.Compilation;
using Ghostly.Features.Querying.Expressions;
using Ghostly.Features.Querying.Expressions.Ast;
using Ghostly.Features.Querying.Expressions.Parsing;
using Ghostly.Features.Querying.Rewriting;
using Ghostly.Features.Querying.Rewriting.Properties;
using ConstantExpression = Ghostly.Features.Querying.Expressions.Ast.ConstantExpression;

namespace Ghostly.Features.Querying
{
    public static class GhostlyQueryLanguage
    {
        private static readonly QueryCompiler _compiler;

        static GhostlyQueryLanguage()
        {
            _compiler = new QueryCompiler();
        }

        public static GhostlyExpression Parse(string query)
        {
            if (!TryParse(query, out var result, out var error))
            {
                throw new GhostlyQueryLanguageException(error);
            }

            return result;
        }

        public static IReadOnlyList<AutoCompleteItem> GetKeywords()
        {
            var result = new List<AutoCompleteItem>();
            result.AddRange(
                PropertyDefinitionCollection.Instance.Definitions
                    .Where(x => x.ShowInAutoComplete)
                    .SelectMany(x => AutoCompleteItem.CreateProperties(x)));
            result.AddRange(AutoCompleteItem.CreateKeywords("is:"));
            result.AddRange(AutoCompleteItem.CreateConstants("true", "false"));

            result.AddRange(AutoCompleteItem.CreateOperators("not", "and", "or", "=", "==", "&&", "||", "!=", "<>", ">=", "<=", ">", "<", "!"));
            return result.OrderBy(x => x.Kind).ThenBy(x => x.Name).ToList();
        }

        public static bool TryParse(string query, out GhostlyExpression expression, out string error)
        {
            try
            {
                if (!ExpressionParser.TryParse(query, out expression, out error))
                {
                    return false;
                }

                // Single string constant?
                if (expression is ConstantExpression constant && constant.Value is string constantValue)
                {
                    if (PropertyDefinitionCollection.Instance.Exist(constantValue))
                    {
                        // Transforms "inbox" => "@inbox" => "@inbox == true"
                        expression = PropertyExpression.Create(constantValue);
                    }
                    else
                    {
                        // Transform this to a string literal instead since it's
                        // not a valid property.
                        expression = new StringLiteralExpression(constantValue);
                    }
                }

                // Single string literal?
                if (expression is StringLiteralExpression stringLiteral)
                {
                    var value = stringLiteral.Value;
                    var builder = new StringBuilder();
                    builder.Append($"@body:'{value}' OR @title:'{value}' OR ");
                    builder.Append($"@owner:'{value}' OR @repo:'{value}' OR ");
                    builder.Append($"@comment:'{value}'");
                    return TryParse(builder.ToString(), out expression, out error);
                }

                // Rewrite expression.
                expression = expression.Accept(ImplicitPropertyFixer.Instance);
                expression = expression.Accept(PropertySimplifier.Instance);
                expression = expression.Accept(ImplicitBooleanFixer.Instance);

                // Rewrite certain properties.
                expression = expression.Accept(InvolvedRewriter.Instance);
                expression = expression.Accept(MentionRewriter.Instance);

                // Rewrite collections.
                expression = expression.Accept(CollectionRewriter.Instance);

                // Single property?
                if (expression is PropertyExpression)
                {
                    // Transforms "@inbox" => "@inbox==true"
                    expression = new ComparisonExpression(ComparisonOperator.Equals, expression,
                        new ConstantExpression(typeof(bool), true));
                }

                // Single collection?
                if (expression is CollectionExpression collection)
                {
                    throw new GhostlyQueryLanguageException(
                        $"The property '{collection.Name}' does not evaluate to a boolean result.");
                }

                return true;
            }
            catch (Exception ex)
            {
                expression = null;
                error = ex.Message;
                return false;
            }
        }

        public static bool TryCompile(string query, out Expression<Func<NotificationData, bool>> compiled, out string error)
        {
            if (!TryParse(query, out var expression, out error))
            {
                compiled = null;
                return false;
            }

            return TryCompile(expression, out compiled, out error);
        }

        public static bool TryCompile(GhostlyExpression query, out Expression<Func<NotificationData, bool>> compiled, out string error)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return _compiler.TryCompile(query, out compiled, out error);
        }
    }
}

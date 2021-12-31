using System;
using System.Globalization;
using Ghostly.Features.Querying.Expressions.Ast;
using Superpower;
using Superpower.Parsers;

namespace Ghostly.Features.Querying.Expressions.Parsing
{
    internal static class ExpressionParser
    {
        public static bool TryParse(string expression, out GhostlyExpression root, out string error)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var tokenList = ExpressionTokenizer.Instance.TryTokenize(expression);
            if (!tokenList.HasValue)
            {
                error = tokenList.ToString();
                root = null;
                return false;
            }

            var result = Expression.AtEnd().TryParse(tokenList.Value);
            if (!result.HasValue)
            {
                error = result.ToString();
                root = null;
                return false;
            }

            root = result.Value;
            error = null;
            return true;
        }

        // Parsers
        private static TokenListParser<ExpressionToken, ConnectiveOperator> And => Token.EqualTo(ExpressionToken.And).Value(ConnectiveOperator.And);
        private static TokenListParser<ExpressionToken, ConnectiveOperator> Or => Token.EqualTo(ExpressionToken.Or).Value(ConnectiveOperator.Or);
        private static TokenListParser<ExpressionToken, ComparisonOperator> Lte => Token.EqualTo(ExpressionToken.LessThanOrEquals).Value(ComparisonOperator.LessThanOrEquals);
        private static TokenListParser<ExpressionToken, ComparisonOperator> Neq => Token.EqualTo(ExpressionToken.NotEquals).Value(ComparisonOperator.NotEquals);
        private static TokenListParser<ExpressionToken, ComparisonOperator> Lt => Token.EqualTo(ExpressionToken.LessThan).Value(ComparisonOperator.LessThan);
        private static TokenListParser<ExpressionToken, ComparisonOperator> Gte => Token.EqualTo(ExpressionToken.GreaterThanOrEquals).Value(ComparisonOperator.GreaterThanOrEquals);
        private static TokenListParser<ExpressionToken, ComparisonOperator> Gt => Token.EqualTo(ExpressionToken.GreaterThan).Value(ComparisonOperator.GreaterThan);
        private static TokenListParser<ExpressionToken, ComparisonOperator> Eq => Token.EqualTo(ExpressionToken.Equals).Value(ComparisonOperator.Equals);
        private static TokenListParser<ExpressionToken, ComparisonOperator> Contains => Token.EqualTo(ExpressionToken.Colon).Value(ComparisonOperator.Contains);

        // Expressions
        private static TokenListParser<ExpressionToken, GhostlyExpression> Expression => Disjunction;

        private static TokenListParser<ExpressionToken, GhostlyExpression> Disjunction =>
            Parse.Chain(Or, Conjunction, ConnectiveExpression.Create);

        private static TokenListParser<ExpressionToken, GhostlyExpression> Conjunction =>
            Parse.Chain(And, Negation, ConnectiveExpression.Create);

        private static TokenListParser<ExpressionToken, GhostlyExpression> Negation =>
            (from op in Token.EqualTo(ExpressionToken.Not)
             from com in Comparison
             select (GhostlyExpression)new NotExpression(com))
                .Or(Comparison);

        private static TokenListParser<ExpressionToken, GhostlyExpression> Comparison =>
            Parse.Chain(Lte.Or(Neq.Or(Lt)).Or(Gte.Or(Gt)).Or(Eq).Or(Contains), Operand, ComparisonExpression.Create);

        private static TokenListParser<ExpressionToken, GhostlyExpression> Operand =>
            (from op in Token.EqualTo(ExpressionToken.Not)
             from factor in Factor
             select (GhostlyExpression)new NotExpression(factor))
                .Or(Factor).Named("expression");

        private static TokenListParser<ExpressionToken, GhostlyExpression> Factor =>
            (from lparen in Token.EqualTo(ExpressionToken.LParen)
             from expr in Parse.Ref(() => Expression)
             from rparen in Token.EqualTo(ExpressionToken.RParen)
             select expr).Or(Item);

        private static TokenListParser<ExpressionToken, GhostlyExpression> Item =>
            Literal.Try().Or(Property);

        private static TokenListParser<ExpressionToken, GhostlyExpression> Literal =>
            String
                .Or(StringLiteral)
                .Or(Number)
                .Or(Token.EqualTo(ExpressionToken.True).Value((GhostlyExpression)new ConstantExpression(typeof(bool), true)))
                .Or(Token.EqualTo(ExpressionToken.False).Value((GhostlyExpression)new ConstantExpression(typeof(bool), false)))
                .Named("literal");

        private static TokenListParser<ExpressionToken, GhostlyExpression> Property =>
            Token.EqualTo(ExpressionToken.Property)
                .Select(token => PropertyExpression.Create(token))
                .Named("property");

        private static TokenListParser<ExpressionToken, GhostlyExpression> Number =>
            Token.EqualTo(ExpressionToken.Integer)
                .Apply(Numerics.Integer)
                .SelectCatch(n => int.Parse(n.ToStringValue(), CultureInfo.InvariantCulture), "The numeric literal is too large")
                .Select(d => (GhostlyExpression)new ConstantExpression(typeof(int), d));

        private static TokenListParser<ExpressionToken, GhostlyExpression> String =>
            Token.EqualTo(ExpressionToken.Word)
                .Select(t => (GhostlyExpression)new ConstantExpression(typeof(string), t.ToStringValue().Trim('\"').Trim('\'')));

        private static TokenListParser<ExpressionToken, GhostlyExpression> StringLiteral =>
            Token.EqualTo(ExpressionToken.StringLiteral)
                .Select(t => (GhostlyExpression)new StringLiteralExpression(t.ToStringValue().Trim('\"').Trim('\'')));
    }
}

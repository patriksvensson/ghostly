using System;
using System.Collections.Generic;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace Ghostly.Features.Querying.Expressions.Parsing
{
    internal sealed class ExpressionTokenizer : Tokenizer<ExpressionToken>
    {
        public static ExpressionTokenizer Instance { get; }

        static ExpressionTokenizer()
        {
            Instance = new ExpressionTokenizer();
        }

        private static readonly ExpressionKeyword[] _keywords =
        {
            new ExpressionKeyword("not", ExpressionToken.Not),
            new ExpressionKeyword("and", ExpressionToken.And),
            new ExpressionKeyword("or", ExpressionToken.Or),
            new ExpressionKeyword("true", ExpressionToken.True),
            new ExpressionKeyword("false", ExpressionToken.False),
            new ExpressionKeyword("yes", ExpressionToken.True),
            new ExpressionKeyword("no", ExpressionToken.False),
        };

        private static TextParser<ExpressionToken> LessOrEqual => Span.EqualTo("<=").Value(ExpressionToken.LessThanOrEquals);
        private static TextParser<ExpressionToken> GreaterOrEqual => Span.EqualTo(">=").Value(ExpressionToken.GreaterThanOrEquals);
        private static TextParser<ExpressionToken> NotEqual => Span.EqualTo("!=").Or(Span.EqualTo("<>")).Value(ExpressionToken.NotEquals);
        private static TextParser<ExpressionToken> Equal => Span.EqualTo("==").Value(ExpressionToken.Equals);
        private static TextParser<ExpressionToken> And => Span.EqualTo("&&").Value(ExpressionToken.And);
        private static TextParser<ExpressionToken> Or => Span.EqualTo("||").Value(ExpressionToken.Or);

        public static TextParser<ExpressionToken> CompoundOperator =>
            GreaterOrEqual.Or(LessOrEqual.Try().Or(NotEqual).Try().Or(Equal).Try()
                .Or(And).Try().Or(Or));

        private struct ExpressionKeyword
        {
            public string Text { get; }
            public ExpressionToken Token { get; }

            public ExpressionKeyword(string text, ExpressionToken token)
            {
                Text = text ?? throw new ArgumentNullException(nameof(text));
                Token = token;
            }
        }

        protected override IEnumerable<Result<ExpressionToken>> Tokenize(
            TextSpan span, TokenizationState<ExpressionToken> tokenizationState)
        {
            var next = SkipWhiteSpace(span);
            if (!next.HasValue)
            {
                yield break;
            }

            do
            {
                // Words or keywords
                if (char.IsLetter(next.Value))
                {
                    var beginIdentifier = ParseIdentifier(ref next);

                    var nextFail = next.HasValue && char.IsLetterOrDigit(next.Value);
                    if (nextFail)
                    {
                        yield return Result.Empty<ExpressionToken>(next.Location);
                    }
                    else
                    {
                        if (TryGetKeyword(beginIdentifier.Until(next.Location), out var keyword))
                        {
                            // Keyword
                            yield return Result.Value(keyword, beginIdentifier, next.Location);
                        }
                        else
                        {
                            // Constant
                            yield return Result.Value(ExpressionToken.Word, beginIdentifier, next.Location);
                        }
                    }
                }

                // Property
                else if (next.Value == '@')
                {
                    next = next.Remainder.ConsumeChar();
                    var beginIdentifier = ParseIdentifier(ref next);
                    yield return Result.Value(ExpressionToken.Property, beginIdentifier, next.Location);
                }

                // Digit
                else if (char.IsDigit(next.Value))
                {
                    var real = Numerics.Integer(next.Location);
                    if (!real.HasValue)
                    {
                        yield return Result.CastEmpty<TextSpan, ExpressionToken>(real);
                    }
                    else
                    {
                        yield return Result.Value(ExpressionToken.Integer, real.Location, real.Remainder);
                        next = real.Remainder.ConsumeChar();
                    }

                    if (!IsDelimiter(next))
                    {
                        yield return Result.Empty<ExpressionToken>(next.Location, new[] { "digit" });
                    }
                }

                // "
                else if (next.Value == '"' || next.Value == '\'')
                {
                    var quoted = next.Value == '"' ? QuotedString.CStyle(next.Location) : QuotedString.SqlStyle(next.Location);
                    if (!quoted.HasValue)
                    {
                        yield return Result.CastEmpty<string, ExpressionToken>(quoted);
                    }
                    else
                    {
                        yield return Result.Value(ExpressionToken.StringLiteral, quoted.Location, quoted.Remainder);
                        next = quoted.Remainder.ConsumeChar();
                    }
                }
                else
                {
                    var compoundOp = CompoundOperator(next.Location);
                    if (compoundOp.HasValue)
                    {
                        yield return Result.Value(compoundOp.Value, compoundOp.Location, compoundOp.Remainder);
                        next = compoundOp.Remainder.ConsumeChar();
                    }

                    // (
                    else if (next.Value == '(')
                    {
                        yield return Result.Value(ExpressionToken.LParen, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }

                    // )
                    else if (next.Value == ')')
                    {
                        yield return Result.Value(ExpressionToken.RParen, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }

                    // =
                    else if (next.Value == '=')
                    {
                        yield return Result.Value(ExpressionToken.Equals, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }

                    // :
                    else if (next.Value == ':')
                    {
                        yield return Result.Value(ExpressionToken.Colon, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }

                    // !
                    else if (next.Value == '!')
                    {
                        yield return Result.Value(ExpressionToken.Not, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }

                    // <
                    else if (next.Value == '<')
                    {
                        yield return Result.Value(ExpressionToken.LessThan, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }

                    // >
                    else if (next.Value == '>')
                    {
                        yield return Result.Value(ExpressionToken.GreaterThan, next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else
                    {
                        yield return Result.Empty<ExpressionToken>(next.Location);
                        next = next.Remainder.ConsumeChar();
                    }
                }

                next = SkipWhiteSpace(next.Location);
            }
            while (next.HasValue);
        }

        private bool IsDelimiter(Result<char> next)
        {
            return !next.HasValue || char.IsWhiteSpace(next.Value) || next.Value == ')';
        }

        private static TextSpan ParseIdentifier(ref Result<char> next)
        {
            var beginIdentifier = next.Location;
            do
            {
                next = next.Remainder.ConsumeChar();
            }
            while (next.HasValue && (char.IsLetterOrDigit(next.Value) || next.Value == '_' || next.Value == '-' || next.Value == '.'));
            return beginIdentifier;
        }

        private static bool TryGetKeyword(TextSpan span, out ExpressionToken keyword)
        {
            foreach (var kw in _keywords)
            {
                if (span.EqualsValueIgnoreCase(kw.Text))
                {
                    keyword = kw.Token;
                    return true;
                }
            }

            keyword = ExpressionToken.None;
            return false;
        }
    }
}

using System;
using System.Diagnostics.CodeAnalysis;
using Superpower;
using Superpower.Model;

namespace Ghostly.Features.Querying.Expressions
{
    internal static class SuperpowerExtensions
    {
        public static TokenListParser<TTokenKind, TResult> SelectCatch<TTokenKind, T, TResult>(
            this TokenListParser<TTokenKind, T> parser,
            Func<T, TResult> trySelector, string errorMessage)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (trySelector == null)
            {
                throw new ArgumentNullException(nameof(trySelector));
            }

            if (errorMessage == null)
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            return input =>
            {
                var t = parser(input);
                if (!t.HasValue)
                {
                    return TokenListParserResult.CastEmpty<TTokenKind, T, TResult>(t);
                }

                try
                {
                    var u = trySelector(t.Value);
                    return TokenListParserResult.Value(u, input, t.Remainder);
                }
                catch
                {
                    return TokenListParserResult.Empty<TTokenKind, TResult>(input, errorMessage);
                }
            };
        }
    }
}

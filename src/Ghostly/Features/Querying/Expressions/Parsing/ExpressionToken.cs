using Superpower.Display;

namespace Ghostly.Features.Querying.Expressions.Parsing
{
    internal enum ExpressionToken
    {
        None,
        [Token(Category = "literal")]
        Word,
        [Token(Category = "literal")]
        StringLiteral,
        [Token(Category = "literal")]
        Integer,
        [Token(Category = "property")]
        Property,
        [Token(Category = "keyword", Example = "AND")]
        And,
        [Token(Category = "keyword", Example = "OR")]
        Or,
        [Token(Category = "keyword")]
        True,
        [Token(Category = "keyword")]
        False,
        [Token(Category = "operator", Example = "==")]
        Equals,
        [Token(Category = "operator", Example = "!=")]
        NotEquals,
        [Token(Category = "operator", Example = ">")]
        GreaterThan,
        [Token(Category = "operator", Example = ">=")]
        GreaterThanOrEquals,
        [Token(Category = "operator", Example = "<")]
        LessThan,
        [Token(Category = "operator", Example = "<=")]
        LessThanOrEquals,
        [Token(Example = "(")]
        LParen,
        [Token(Example = ")")]
        RParen,
        [Token(Category = "operator", Example = "!")]
        Not,
        [Token(Example = ":")]
        Colon,
    }
}

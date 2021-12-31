using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class ReadProperty : PropertyDefinition<bool>
    {
        public override string Glyph => "\uE8C3";
        public override string LocalizedDescription => "Query_Property_Read";

        public ReadProperty()
            : base("read")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            var unread = context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.Unread));
            return Expression.Not(unread);
        }
    }
}

using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class UnreadProperty : PropertyDefinition<bool>
    {
        public override string Glyph => "\uE715";
        public override string LocalizedDescription => "Query_Property_Unread";

        public UnreadProperty()
            : base("unread")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.Unread));
        }
    }
}

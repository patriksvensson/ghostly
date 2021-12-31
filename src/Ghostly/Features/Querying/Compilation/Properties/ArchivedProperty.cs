using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class ArchivedProperty : PropertyDefinition<bool>
    {
        public override string Glyph => "\uE7B8";
        public override string LocalizedDescription => "Query_Property_Archived";

        public ArchivedProperty()
            : base("archived")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.Category.Archive));
        }
    }
}

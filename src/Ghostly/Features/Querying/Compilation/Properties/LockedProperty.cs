using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class LockedProperty : PropertyDefinition<bool>
    {
        public override string Glyph => "\uE72E";
        public override string LocalizedDescription => "Query_Property_Locked";

        public LockedProperty()
            : base("locked")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.Locked));
        }
    }
}

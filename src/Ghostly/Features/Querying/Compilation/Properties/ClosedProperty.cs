using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class ClosedProperty : PropertyDefinition<bool>
    {
        public override string Glyph => "\uE8BB";
        public override string LocalizedDescription => "Query_Property_Closed";

        public ClosedProperty()
            : base("closed")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.IsClosed));
        }
    }
}

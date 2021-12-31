using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class DraftProperty : PropertyDefinition<bool>
    {
        public override string LocalizedDescription => "Query_Property_Draft";

        public DraftProperty()
            : base("draft")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.IsDraft));
        }
    }
}

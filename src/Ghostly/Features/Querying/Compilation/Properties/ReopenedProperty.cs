using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class ReopenedProperty : PropertyDefinition<bool>
    {
        public override string LocalizedDescription => "Query_Property_Reopened";

        public ReopenedProperty()
            : base("reopened")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.IsReopened));
        }
    }
}

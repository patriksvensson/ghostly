using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class OwnerProperty : PropertyDefinition<string>
    {
        public override string LocalizedDescription => "Query_Property_Owner";

        public OwnerProperty()
            : base("owner", "org")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.Repository.Owner));
        }
    }
}

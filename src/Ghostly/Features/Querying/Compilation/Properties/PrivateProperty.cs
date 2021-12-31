using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class PrivateProperty : PropertyDefinition<bool>
    {
        public override string LocalizedDescription => "Query_Property_Private";

        public PrivateProperty()
            : base("private")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.Repository.Private));
        }
    }
}

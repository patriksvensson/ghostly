using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class MergedByProperty : PropertyDefinition<string>
    {
        public override string LocalizedDescription => "Query_Property_MergedBy";
        public override string LocalizedType => "Query_Type_Username";

        public MergedByProperty()
            : base("merger")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.MergedBy.Login));
        }
    }
}

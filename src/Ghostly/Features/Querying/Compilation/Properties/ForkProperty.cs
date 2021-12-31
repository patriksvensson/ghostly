using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class ForkProperty : PropertyDefinition<bool>
    {
        public override string LocalizedDescription => "Query_Property_Fork";

        public ForkProperty()
            : base("fork")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.Repository.Fork));
        }
    }
}

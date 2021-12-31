using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class ReleaseProperty : PropertyDefinition<bool>
    {
        public override string LocalizedDescription => "Query_Property_Release";

        public ReleaseProperty()
            : base("release")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.IsRelease));
        }
    }
}

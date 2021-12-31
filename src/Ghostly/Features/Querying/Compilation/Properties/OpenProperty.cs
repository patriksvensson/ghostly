using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class OpenProperty : PropertyDefinition<bool>
    {
        public override string LocalizedDescription => "Query_Property_Open";

        public OpenProperty()
            : base("open")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.IsOpen));
        }
    }
}

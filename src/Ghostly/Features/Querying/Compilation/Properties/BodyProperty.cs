using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class BodyProperty : PropertyDefinition<string>
    {
        public override string LocalizedDescription => "Query_Property_Body";

        public BodyProperty()
            : base("body")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.Body));
        }
    }
}

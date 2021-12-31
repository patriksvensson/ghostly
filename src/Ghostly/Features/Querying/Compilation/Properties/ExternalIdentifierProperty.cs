using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class ExternalIdentifierProperty : PropertyDefinition<int>
    {
        public override string LocalizedDescription => "Query_Property_Id";

        public ExternalIdentifierProperty()
            : base("id")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.GitHubLocalId));
        }
    }
}

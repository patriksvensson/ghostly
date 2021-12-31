using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class RepositoryProperty : PropertyDefinition<string>
    {
        public override string LocalizedDescription => "Query_Property_Repository";

        public RepositoryProperty()
            : base("repo", "repository")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.Repository.Name));
        }
    }
}

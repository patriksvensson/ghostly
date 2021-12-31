using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class CommitProperty : PropertyDefinition<bool>
    {
        public override string LocalizedDescription => "Query_Property_Commit";

        public CommitProperty()
            : base("commit")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.IsCommit));
        }
    }
}

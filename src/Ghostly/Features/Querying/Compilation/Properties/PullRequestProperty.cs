using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class PullRequestProperty : PropertyDefinition<bool>
    {
        public override string LocalizedDescription => "Query_Property_PullRequest";

        public PullRequestProperty()
            : base("pullrequest", "pr")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.IsPullRequest));
        }
    }
}

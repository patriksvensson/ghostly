using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class IssueProperty : PropertyDefinition<bool>
    {
        public override string LocalizedDescription => "Query_Property_Issue";

        public IssueProperty()
            : base("issue")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.IsIssue));
        }
    }
}

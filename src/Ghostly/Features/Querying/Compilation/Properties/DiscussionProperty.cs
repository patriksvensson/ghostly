using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class DiscussionProperty : PropertyDefinition<bool>
    {
        public override string LocalizedDescription => "Query_Property_Discussion";

        public DiscussionProperty()
            : base("discussion")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.IsDiscussion));
        }
    }
}

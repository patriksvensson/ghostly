using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class ReviewCountProperty : PropertyDefinition<int>
    {
        public override bool ShowInAutoComplete => false;

        public ReviewCountProperty()
            : base("review_count")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.Reviews.Count));
        }
    }
}

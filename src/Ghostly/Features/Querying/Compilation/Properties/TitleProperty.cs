using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class TitleProperty : PropertyDefinition<string>
    {
        public override string LocalizedDescription => "Query_Property_Title";

        public TitleProperty()
            : base("title")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.Title));
        }
    }
}

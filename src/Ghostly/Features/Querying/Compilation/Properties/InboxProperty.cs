using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class InboxProperty : PropertyDefinition<bool>
    {
        public override string LocalizedDescription => "Query_Property_Inbox";

        public InboxProperty()
            : base("inbox")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.Category.Inbox));
        }
    }
}

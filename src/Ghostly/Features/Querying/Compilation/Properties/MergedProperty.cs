using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class MergedProperty : PropertyDefinition<bool>
    {
        public override string Glyph => "\uEA3C";
        public override string LocalizedDescription => "Query_Property_Merged";

        public MergedProperty()
            : base("merged")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.Merged));
        }
    }
}

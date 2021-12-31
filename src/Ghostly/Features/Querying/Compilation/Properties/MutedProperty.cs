using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class MutedProperty : PropertyDefinition<bool>
    {
        public override string Glyph => "\uE74F";
        public override string LocalizedDescription => "Query_Property_Muted";

        public MutedProperty()
            : base("muted")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.Muted));
        }
    }
}

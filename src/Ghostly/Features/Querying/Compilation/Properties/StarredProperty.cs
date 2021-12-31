using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class StarredProperty : PropertyDefinition<bool>
    {
        public override string Glyph => "\uE734";
        public override string LocalizedDescription => "Query_Property_Starred";

        public StarredProperty()
            : base("starred")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.Starred));
        }
    }
}

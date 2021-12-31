using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class CategoryProperty : PropertyDefinition<string>
    {
        public override string Glyph => Constants.Glyphs.Category;
        public override string LocalizedDescription => "Query_Property_Category";

        public CategoryProperty()
            : base("category")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.Category.Name));
        }
    }
}

using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class AuthorProperty : PropertyDefinition<string>
    {
        public override string Glyph => "\uE77B";
        public override string LocalizedDescription => "Query_Property_Author";
        public override string LocalizedType => "Query_Type_Username";

        public AuthorProperty()
            : base("author")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.Author.Login));
        }
    }
}

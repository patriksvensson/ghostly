using Ghostly.Features.Querying.Compilation;
using Ghostly.Features.Querying.Expressions;
using Ghostly.Features.Querying.Expressions.Ast;

namespace Ghostly.Features.Querying.Rewriting
{
    public sealed class CollectionRewriter : QueryRewriter<object>
    {
        public static CollectionRewriter Instance { get; } = new CollectionRewriter();

        protected override GhostlyExpression Visit(PropertyExpression expression, object context)
        {
            if (PropertyDefinitionCollection.Instance.TryGet(expression.Name, out var definition))
            {
                if (definition is CollectionDefinition)
                {
                    return new CollectionExpression(expression.Name);
                }
            }

            return expression;
        }
    }
}

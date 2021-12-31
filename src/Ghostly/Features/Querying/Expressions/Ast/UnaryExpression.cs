using System;

namespace Ghostly.Features.Querying.Expressions.Ast
{
    public abstract class UnaryExpression : GhostlyExpression
    {
        public GhostlyExpression Operand { get; }

        protected UnaryExpression(GhostlyExpression operand)
        {
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        }
    }
}

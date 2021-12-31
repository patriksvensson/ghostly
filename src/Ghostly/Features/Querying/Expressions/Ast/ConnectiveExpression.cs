using System;

namespace Ghostly.Features.Querying.Expressions.Ast
{
    public abstract class ConnectiveExpression : BinaryExpression
    {
        protected ConnectiveExpression(GhostlyExpression left, GhostlyExpression right)
            : base(left, right)
        {
        }

        internal static GhostlyExpression Create(ConnectiveOperator @operator, GhostlyExpression left, GhostlyExpression right)
        {
            if (@operator == ConnectiveOperator.And)
            {
                return new AndExpression(left, right);
            }

            if (@operator == ConnectiveOperator.Or)
            {
                return new OrExpression(left, right);
            }

            throw new NotSupportedException("Connective operator is not supported.");
        }
    }
}

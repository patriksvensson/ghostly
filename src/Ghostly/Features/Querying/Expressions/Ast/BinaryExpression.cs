namespace Ghostly.Features.Querying.Expressions.Ast
{
    public abstract class BinaryExpression : GhostlyExpression
    {
        public GhostlyExpression Left { get; }
        public GhostlyExpression Right { get; }

        protected BinaryExpression(GhostlyExpression left, GhostlyExpression right)
        {
            Left = left;
            Right = right;
        }
    }
}

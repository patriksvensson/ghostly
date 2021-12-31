using System.Linq.Expressions;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class MilestoneProperty : PropertyDefinition<string>
    {
        public override string LocalizedDescription => "Query_Property_Milestone";

        public MilestoneProperty()
            : base("milestone")
        {
        }

        public override Expression CompileMember(QueryCompilerContext context)
        {
            return context.MakeMemberAccess(ExpressionHelper.GetPropertyPath(n => n.WorkItem.Milestone.Name));
        }
    }
}

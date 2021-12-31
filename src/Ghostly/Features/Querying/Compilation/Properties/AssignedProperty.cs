using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Ghostly.Data.Models;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class AssignedProperty : CollectionDefinition<AssigneeData, string>
    {
        public override string Glyph => "\uE77B";
        public override string LocalizedDescription => "Query_Property_Assigned";
        public override string LocalizedType => "Query_Type_Username";

        public AssignedProperty()
            : base("assigned", "assignee")
        {
        }

        protected override Expression<Func<NotificationData, List<AssigneeData>>> GetCollection()
        {
            return notification => notification.WorkItem.Assignees;
        }

        protected override Expression<Func<AssigneeData, string>> GetMember()
        {
            return assignee => assignee.User.Login;
        }
    }
}

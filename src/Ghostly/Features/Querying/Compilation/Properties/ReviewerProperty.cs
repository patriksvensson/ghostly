using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Ghostly.Data.Models;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class ReviewerProperty : CollectionDefinition<ReviewData, string>
    {
        public override string LocalizedDescription => "Query_Property_Reviewer";
        public override string LocalizedType => "Query_Type_Username";

        public ReviewerProperty()
            : base("reviewer")
        {
        }

        protected override Expression<Func<NotificationData, List<ReviewData>>> GetCollection()
        {
            return notification => notification.WorkItem.Reviews;
        }

        protected override Expression<Func<ReviewData, string>> GetMember()
        {
            return review => review.Author.Login;
        }
    }
}

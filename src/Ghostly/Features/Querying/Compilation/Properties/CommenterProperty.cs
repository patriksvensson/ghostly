using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Ghostly.Data.Models;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class CommenterProperty : CollectionDefinition<CommentData, string>
    {
        public override string LocalizedDescription => "Query_Property_Commenter";
        public override string LocalizedType => "Query_Type_Username";

        public CommenterProperty()
            : base("commenter")
        {
        }

        protected override Expression<Func<NotificationData, List<CommentData>>> GetCollection()
        {
            return notification => notification.WorkItem.Comments;
        }

        protected override Expression<Func<CommentData, string>> GetMember()
        {
            return comment => comment.Author.Login;
        }
    }
}

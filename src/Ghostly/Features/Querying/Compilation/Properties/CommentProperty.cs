using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Ghostly.Data.Models;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class CommentProperty : CollectionDefinition<CommentData, string>
    {
        public override string Glyph => "\uE90A";
        public override string LocalizedDescription => "Query_Property_Comment";

        public CommentProperty()
            : base("comment")
        {
        }

        protected override Expression<Func<NotificationData, List<CommentData>>> GetCollection()
        {
            return notification => notification.WorkItem.Comments;
        }

        protected override Expression<Func<CommentData, string>> GetMember()
        {
            return comment => comment.Body;
        }
    }
}

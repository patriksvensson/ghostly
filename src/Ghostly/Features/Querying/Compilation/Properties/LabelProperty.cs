using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Ghostly.Data.Models;

namespace Ghostly.Features.Querying.Compilation.Properties
{
    internal sealed class LabelProperty : CollectionDefinition<WorkItemTagData, string>
    {
        public override string Glyph => "\uE8EC";
        public override string LocalizedDescription => "Query_Property_Label";

        public LabelProperty()
            : base("label", "tag")
        {
        }

        protected override Expression<Func<NotificationData, List<WorkItemTagData>>> GetCollection()
        {
            return notification => notification.WorkItem.Tags;
        }

        protected override Expression<Func<WorkItemTagData, string>> GetMember()
        {
            return tag => tag.Tag.Name;
        }
    }
}

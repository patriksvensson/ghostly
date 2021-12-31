using System;
using System.Linq.Expressions;
using Ghostly.Data.Models;

namespace Ghostly.Domain
{
    public sealed class Category
    {
        public int Id { get; set; }
        public string Identifier { get; set; }

        public string Name { get; set; }
        public string Glyph { get; set; }
        public string Emoji { get; set; }
        public string Expression { get; set; }
        public Expression<Func<NotificationData, bool>> Filter { get; set; }

        public CategoryKind Kind { get; set; }
        public int SortOrder { get; set; }

        public bool Muted { get; set; }
        public bool IncludeMuted { get; set; }

        public bool Deletable { get; set; }
        public bool Inbox { get; set; }
        public bool Archive { get; set; }

        public bool ShowTotal { get; set; }

        public string ImportedFrom { get; set; }
        public DateTime? ImportedAt { get; set; }
    }
}

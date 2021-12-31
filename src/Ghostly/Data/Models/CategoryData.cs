using System;
using Ghostly.Domain;

namespace Ghostly.Data.Models
{
    public sealed class CategoryData : EntityData
    {
        public string Name { get; set; }
        public string Identifier { get; set; }

        public string Glyph { get; set; }
        public string Emoji { get; set; }
        public string Expression { get; set; }

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

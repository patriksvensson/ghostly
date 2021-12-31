using System;

namespace Ghostly.Data.Models
{
    public sealed class RuleData : EntityData
    {
        public string Name { get; set; }
        public string Identifier { get; set; }
        public int SortOrder { get; set; }
        public bool Enabled { get; set; }
        public string Expression { get; set; }
        public bool Star { get; set; }
        public bool Mute { get; set; }
        public bool MarkAsRead { get; set; }
        public bool StopProcessing { get; set; }
        public CategoryData Category { get; set; }
        public string ImportedFrom { get; set; }
        public DateTime? ImportedAt { get; set; }
    }
}

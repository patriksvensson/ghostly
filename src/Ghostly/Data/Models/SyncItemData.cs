using System;

namespace Ghostly.Data.Models
{
    public sealed class SyncItemData : EntityData
    {
        public Discriminator Discriminator { get; set; }
        public string Identity { get; set; }
        public DateTime Timestamp { get; set; }
        public string Payload { get; set; }
    }
}

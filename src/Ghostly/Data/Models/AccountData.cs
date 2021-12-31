using System;

namespace Ghostly.Data.Models
{
    public sealed class AccountData : EntityData
    {
        public Discriminator Discriminator { get; set; }

        // Generic
        public AccountState State { get; set; }
        public DateTime? LastSyncedAt { get; set; }

        // GitHub specific
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public string Scopes { get; set; }
    }
}

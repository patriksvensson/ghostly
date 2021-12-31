using System;

namespace Ghostly.Data.Models
{
    public sealed class CommentData : EntityData
    {
        public Discriminator Discriminator { get; set; }

        // GitHub specific
        public int? GitHubId { get; set; }
        public string Url { get; set; }
        public string Body { get; set; }
        public UserData Author { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

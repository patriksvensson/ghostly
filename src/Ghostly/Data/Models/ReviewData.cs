using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ghostly.Data.Models
{
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Data")]
    public sealed class ReviewData : EntityData
    {
        public Discriminator Discriminator { get; set; }

        public long GitHubId { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserData Author { get; set; }
        public ReviewState State { get; set; }
        public string Url { get; set; }
        public string Body { get; set; }
        public List<ReviewCommentData> Comments { get; set; }
    }
}

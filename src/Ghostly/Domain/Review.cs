using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Ghostly.Data.Models;

namespace Ghostly.Domain
{
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Data")]
    public sealed class Review : Entity
    {
        public long GitHubId { get; set; }
        public DateTime CreatedAt { get; set; }
        public User Author { get; set; }
        public ReviewState State { get; set; }
        public Uri Url { get; set; }
        public string Body { get; set; }
        public List<ReviewComment> Comments { get; set; }
    }
}

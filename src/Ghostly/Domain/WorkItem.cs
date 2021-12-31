using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Ghostly.Data.Models;

namespace Ghostly.Domain
{
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Data")]
    public abstract class WorkItem : Entity
    {
        public Uri Url { get; set; }

        public abstract DateTime Timestamp { get; }
        public abstract string Origin { get; }

        public string Title { get; set; }
        public string Preamble { get; set; }
        public string Body { get; set; }

        public List<Tag> Tags { get; set; }
        public Milestone Milestone { get; set; }

        public WorkItemState State { get; set; }
        public User Author { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Review> Reviews { get; set; }
    }
}

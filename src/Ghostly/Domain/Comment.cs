using System;

namespace Ghostly.Domain
{
    public abstract class Comment : Entity
    {
        public Uri Url { get; set; }

        public User Author { get; set; }
        public string Body { get; set; }

        public abstract DateTime Timestamp { get; }
    }
}

using System;
using Ghostly.Core;
using Ghostly.Domain;

namespace Ghostly.Services.Templating.Objects
{
    public sealed class TemplateReviewItemComment : IHaveTimestamp
    {
        public User Author { get; set; }
        public DateTime Timestamp { get; set; }
        public string Body { get; set; }
    }
}

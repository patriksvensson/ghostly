using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ghostly.Services.Templating.Objects
{
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Data")]
    public sealed class TemplateReviewItem
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public string Diff { get; set; }
        public List<TemplateReviewItemComment> Comments { get; set; }

        public TemplateReviewItem()
        {
            Comments = new List<TemplateReviewItemComment>();
        }
    }
}

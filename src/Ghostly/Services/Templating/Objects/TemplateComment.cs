using System;
using Ghostly.Core.Services;
using Ghostly.Domain;

namespace Ghostly.Services.Templating.Objects
{
    public sealed class TemplateComment : ITemplateItem
    {
        public bool IsReview => false;
        public Comment Comment { get; }
        public User Author => Comment.Author;
        public DateTime Timestamp => Comment.Timestamp;
        public string Body => Comment.Body;
        public string Action { get; }
        public string Url => Comment.Url?.ToString();
        public bool ShowBody => true;

        public TemplateComment(ILocalizer localizer, Comment comment)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            Action = localizer.GetString("Template_Action_Commented").ToLowerInvariant();
            Comment = comment ?? throw new ArgumentNullException(nameof(comment));
        }
    }
}

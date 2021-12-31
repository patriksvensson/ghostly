using System;
using System.Collections.Generic;
using Ghostly.Core.Services;
using Ghostly.Data.Models;
using Ghostly.Domain;

namespace Ghostly.Services.Templating.Objects
{
    public sealed class TemplateReview : ITemplateItem
    {
        public bool IsReview => true;
        public Review Review { get; }
        public User Author => Review.Author;
        public DateTime Timestamp => Review.CreatedAt;
        public ReviewState State => Review.State;
        public string Body => Review.Body;
        public string Action { get; }
        public string Url => Review.Url?.ToString();
        public List<TemplateReviewItem> Items { get; }
        public bool ShowBody => !string.IsNullOrWhiteSpace(Body);

        public TemplateReview(ILocalizer localizer, Review review)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            Review = review ?? throw new ArgumentNullException(nameof(review));
            Action = GetAction(localizer);
            Items = new List<TemplateReviewItem>();
        }

        private string GetAction(ILocalizer localizer)
        {
            switch (State)
            {
                case ReviewState.Unknown:
                    return localizer.GetString("Template_Action_Reviewed");
                case ReviewState.Approved:
                    return localizer.GetString("Template_Action_Approved");
                case ReviewState.ChangesRequested:
                    return localizer.GetString("Template_Action_RequestedChanges");
                case ReviewState.Commented:
                    return localizer.GetString("Template_Action_Commented");
                case ReviewState.Dismissed:
                    return localizer.GetString("Template_Action_Dismissed");
                case ReviewState.Pending:
                    return localizer.GetString("Template_Action_Pending");
                default:
                    return localizer.GetString("Template_Action_Unknown");
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;

namespace Ghostly.Services.Templating.Objects
{
    public class TemplateItemBuilder
    {
        private readonly ILocalizer _localizer;

        public TemplateItemBuilder(ILocalizer localizer)
        {
            _localizer = localizer;
        }

        public List<ITemplateItem> Build(WorkItem workitem)
        {
            if (workitem is null)
            {
                throw new System.ArgumentNullException(nameof(workitem));
            }

            var result = new List<ITemplateItem>();

            foreach (var comment in workitem.Comments)
            {
                result.Add(new TemplateComment(_localizer, comment));
            }

            foreach (var review in BuildAggregatedReviews(workitem))
            {
                if (review.Items.Count == 0 && string.IsNullOrWhiteSpace(review.Body))
                {
                    // This is a review that's a placeholder for a comment.
                    // Comments to a review comment is connected to a new review in GitHub ¯\_(ツ)_/¯
                    continue;
                }

                result.Add(review);
            }

            if (workitem is GitHubWorkItem github)
            {
                if (github.Kind == GitHubWorkItemKind.PullRequest &&
                    github.Merged.GetSafeValue() && github.MergedAt != null &&
                    github.MergedBy != null)
                {
                    result.Add(new TemplateMergedAction(_localizer, github));
                }
            }

            return result;
        }

        private IEnumerable<TemplateReview> BuildAggregatedReviews(WorkItem workitem)
        {
            var reviews = new List<TemplateReview>();
            foreach (var review in workitem.Reviews)
            {
                var reviewItem = new TemplateReview(_localizer, review);
                foreach (var comment in review.Comments.Where(c => c.InReplyToId == null))
                {
                    reviewItem.Items.Add(new TemplateReviewItem
                    {
                        Id = comment.GitHubId,
                        Path = comment.Path,
                        Diff = comment.Diff,
                        Comments = new List<TemplateReviewItemComment>(),
                    });
                }

                reviews.Add(reviewItem);
            }

            // Add comments.
            foreach (var review in reviews)
            {
                foreach (var comment in review.Review.Comments)
                {
                    var parentId = comment.InReplyToId.GetValueOrDefault(comment.GitHubId);
                    var parent = FindReviewItem(reviews, parentId);
                    if (parent != null)
                    {
                        parent.Comments.Add(new TemplateReviewItemComment
                        {
                            Author = comment.Author,
                            Timestamp = comment.CreatedAt,
                            Body = comment.Body,
                        });
                    }
                }
            }

            return reviews;
        }

        private static TemplateReviewItem FindReviewItem(IEnumerable<TemplateReview> reviews, long id)
        {
            foreach (var review in reviews)
            {
                var item = review.Items.SingleOrDefault(i => i.Id == id);
                if (item != null)
                {
                    return item;
                }
            }

            return null;
        }
    }
}

using System;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;

namespace Ghostly.Services.Templating.Objects
{
    public sealed class TemplateMergedAction : ITemplateItem
    {
        public bool IsReview => false;
        public User Author { get; }
        public string Action { get; }
        public string Url { get; }
        public string Body { get; }
        public DateTime Timestamp { get; }
        public bool ShowBody => false;

        public TemplateMergedAction(ILocalizer localizer, GitHubWorkItem workitem)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            if (workitem is null)
            {
                throw new ArgumentNullException(nameof(workitem));
            }

            Author = workitem.MergedBy;
            Action = localizer.GetString("Template_Action_MergedThis").ToLowerInvariant();
            Url = workitem.Url?.ToString();
            Body = null;
            Timestamp = workitem.MergedAt.Value;
        }
    }
}

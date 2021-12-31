using System;
using System.Globalization;
using Ghostly.Domain.GitHub;
using Ghostly.Features.Synchronization;
using Newtonsoft.Json;

namespace Ghostly.GitHub
{
    internal sealed class GitHubNotificationItem : ISynchronizationItem
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }
        [JsonProperty(PropertyName = "work_item_id")]
        public long WorkItemId { get; set; }

        [JsonProperty(PropertyName = "kind")]
        public GitHubWorkItemKind Kind { get; set; }
        [JsonProperty(PropertyName = "reason")]
        public string Reason { get; set; }

        [JsonProperty(PropertyName = "unread")]
        public bool Unread { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty(PropertyName = "last_read_at")]
        public DateTime? LastReadAt { get; set; }
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "repository")]
        public GitHubRepositoryInfo Repository { get; set; }
        [JsonProperty(PropertyName = "subject")]
        public GitHubNotificationSubject Subject { get; set; }

        [JsonIgnore]
        public string Identity => Id.ToString(CultureInfo.InvariantCulture);
        [JsonIgnore]
        public DateTime Timestamp => UpdatedAt;
    }

    public sealed class GitHubNotificationSubject
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
        [JsonProperty(PropertyName = "latest_comment_url")]
        public string LatestCommentUrl { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }

    public sealed class GitHubRepositoryInfo
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }
        [JsonProperty(PropertyName = "owner")]
        public string Owner { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "is_private")]
        public bool IsPrivate { get; set; }
        [JsonProperty(PropertyName = "is_fork")]
        public bool IsFork { get; set; }
        [JsonProperty(PropertyName = "html_url")]
        public string HtmlUrl { get; set; }
    }
}

using System;
using Ghostly.Domain.GitHub;
using Octokit.Internal;
using Shouldly;
using Xunit;
using OctokitNotification = Octokit.Notification;

namespace Ghostly.GitHub.Tests.Unit
{
    public sealed class GitHubNotificationItemMapperTests
    {
        [Theory]
        [EmbeddedResourceData("Ghostly.GitHub.Tests/Data/SingleNotification.json")]
        public void Should_Map_Notification(string json)
        {
            // Given
            var serializer = new SimpleJsonSerializer();
            var notification = serializer.Deserialize<OctokitNotification>(json);

            // When
            var result = GitHubNotificationItemMapper.Map(notification);

            // Then
            result.Id.ShouldBe(1);
            result.Kind.ShouldBe(GitHubWorkItemKind.PullRequest);
            result.WorkItemId.ShouldBe(373);
            result.Reason.ShouldBe("review_requested");
            result.Unread.ShouldBeTrue();
            result.UpdatedAt.ShouldBe(new DateTime(2019, 8, 19, 13, 12, 59, DateTimeKind.Utc));
            result.LastReadAt.ShouldBe(new DateTime(2019, 8, 20, 15, 23, 12, DateTimeKind.Utc));
            result.Url.ShouldBe("https://api.github.com/notifications/threads/564191148");

            result.Repository.Id.ShouldBe(41851619);
            result.Repository.Owner.ShouldBe("patriksvensson");
            result.Repository.Name.ShouldBe("test-repo");
            result.Repository.IsPrivate.ShouldBeTrue();
            result.Repository.IsFork.ShouldBeTrue();

            result.Subject.Title.ShouldBe("This is the title");
            result.Subject.Url.ShouldBe("https://api.github.com/repos/patriksvensson/test-repo/pulls/373");
            result.Subject.LatestCommentUrl.ShouldBe("https://api.github.com/comments/patriksvensson/test-repo/pulls/373");
            result.Subject.Type.ShouldBe("PullRequest");
        }
    }
}

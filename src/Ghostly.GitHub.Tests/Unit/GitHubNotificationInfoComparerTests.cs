using System.Collections.Generic;
using Ghostly.GitHub;
using Shouldly;
using Xunit;

namespace Ghostly.Tests.Unit.GitHub
{
    public sealed class GitHubNotificationInfoComparerTests
    {
        [Fact]
        public void Should_Work_As_Expected_With_HashSet_2()
        {
            // Given, When
            var set = new HashSet<GitHubNotificationItem>(new GitHubNotificationItemComparer())
            {
                new GitHubNotificationItem() { Id = 1 },
                new GitHubNotificationItem() { Id = 1 },
            };

            // Then
            set.Count.ShouldBe(1);
        }
    }
}

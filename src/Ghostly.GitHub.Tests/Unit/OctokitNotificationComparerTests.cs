using System.Collections.Generic;
using Ghostly.GitHub.Octokit;
using Shouldly;
using Xunit;
using OctokitNotification = Octokit.Notification;

namespace Ghostly.Tests.Unit.GitHub
{
    public sealed class OctokitNotificationComparerTests
    {
        [Fact]
        public void Should_Consider_Two_Notifications_With_The_Same_Id_Equal()
        {
            // Given
            var comparer = new OctokitNotificationComparer();

            // When
            var result = comparer.Equals(
                new OctokitNotification("1", null, null, null, true, null, null, null),
                new OctokitNotification("1", null, null, null, false, null, null, null));

            // Then
            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Work_As_Expected_With_HashSet()
        {
            // Given, When
            var set = new HashSet<OctokitNotification>(new OctokitNotificationComparer())
            {
                new OctokitNotification("1", null, null, null, true, null, null, null),
                new OctokitNotification("1", null, null, null, false, null, null, null),
            };

            // Then
            set.Count.ShouldBe(1);
        }

        [Theory]
        [InlineData(1, "FOO")]
        [InlineData(2, "FOO", "BAR")]
        [InlineData(3, "Foo", "BAR", "BAZ")]
        [InlineData(4, "Foo", "BAR", "BAZ", "QUX")]
        [InlineData(5, "Foo", "BAR", "BAZ", "QUX", "CORGI")]
        [InlineData(6, "Foo", "BAR", "BAZ", "QUX", "CORGI", "OCTOPUS")]
        public void Batch(int expected, params string[] items)
        {
            // Given
            var list = new List<string>();
            list.AddRange(items);

            // When
            var result = new List<string>();
            foreach (var group in list.Batch(5))
            {
                foreach (var item in group)
                {
                    result.Add(item);
                }
            }

            // Then
            list.Count.ShouldBe(expected);
        }
    }
}

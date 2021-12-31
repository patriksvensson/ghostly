using System;
using System.Collections.Generic;
using Ghostly.Core.Services;
using Ghostly.Data.Mapping;
using Ghostly.Data.Models;
using Ghostly.Domain.GitHub;
using Ghostly.Testing;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Ghostly.Tests.Unit.Data.Mapping
{
    public sealed class NotificationMapperTests
    {
        [Fact]
        public void Should_Map_Notification_Correctly()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Issue);

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.ShouldBeOfType<GitHubNotification>().And(github =>
            {
                github.Id.ShouldBe(1);
                github.AccountId.ShouldBe(2);
                github.GitHubId.ShouldBe(3);
                github.Muted.ShouldBeFalse();
                github.Starred.ShouldBeTrue();
                github.Timestamp.ShouldBe(new DateTime(2015, 8, 25));
                github.Unread.ShouldBeTrue();
                github.WorkItemId.ShouldBe(4);
                github.CategoryId.ShouldBe(22);
                github.ExternalId.ShouldBe(5);
                github.Origin.ShouldBe("cake-build/cake");
                github.Merged.ShouldBe(true);
                github.Locked.ShouldBe(true);
                github.Preamble.ShouldBe("Bar");
                github.Kind.ShouldBe(GitHubWorkItemKind.Issue);
                github.State.ShouldBe(WorkItemState.Closed);
                github.Title.ShouldBe("Baz");
                github.Url.ShouldBe(new Uri("https://github.com/cake-build/cake/issues/2531", UriKind.RelativeOrAbsolute));
            });
        }

        [Fact]
        public void Should_Map_Notification_As_Read_If_Muted()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Issue);
            data.Muted = true;
            data.Unread = true;

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.ShouldBeOfType<GitHubNotification>().And(github =>
            {
                github.Unread.ShouldBeFalse();
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_Map_Muted_Property(bool muted)
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Issue);
            data.Muted = muted;

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.Muted.ShouldBe(muted);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_Map_Starred_Property(bool starred)
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Issue);
            data.Starred = starred;

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.Starred.ShouldBe(starred);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_Map_Unread_Property(bool unread)
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Issue);
            data.Unread = unread;

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.Unread.ShouldBe(unread);
        }

        [Fact]
        public void Should_Map_IsDraft_Property_Correctly_If_Work_Item_Is_Pull_Request()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.PullRequest);

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.ShouldBeOfType<GitHubNotification>().And(github =>
            {
                github.Draft.ShouldNotBeNull();
                github.Draft.Value.ShouldBeTrue();
            });
        }

        [Fact]
        public void Should_Map_Kind_Property_Correctly_If_Work_Item_Is_Issue()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Issue);

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.ShouldBeOfType<GitHubNotification>().And(github =>
            {
                github.Kind.ShouldBe(GitHubWorkItemKind.Issue);
            });
        }

        [Fact]
        public void Should_Map_Kind_Property_Correctly_If_Work_Item_Is_Pull_Request()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.PullRequest);

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.ShouldBeOfType<GitHubNotification>().And(github =>
            {
                github.Kind.ShouldBe(GitHubWorkItemKind.PullRequest);
            });
        }

        [Fact]
        public void Should_Map_Kind_Property_Correctly_If_Work_Item_Is_Release()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Release);

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.ShouldBeOfType<GitHubNotification>().And(github =>
            {
                github.Kind.ShouldBe(GitHubWorkItemKind.Release);
            });
        }

        [Fact]
        public void Should_Map_Kind_Property_Correctly_If_Work_Item_Is_Vulnerability()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Vulnerability);

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.ShouldBeOfType<GitHubNotification>().And(github =>
            {
                github.Kind.ShouldBe(GitHubWorkItemKind.Vulnerability);
            });
        }

        [Fact]
        public void Should_Map_Kind_Property_Correctly_If_Work_Item_Is_Commit()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Commit);

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.ShouldBeOfType<GitHubNotification>().And(github =>
            {
                github.Kind.ShouldBe(GitHubWorkItemKind.Commit);
            });
        }

        [Fact]
        public void Should_Map_Kind_Property_Correctly_If_Work_Item_Is_Discussion()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Discussion);

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.ShouldBeOfType<GitHubNotification>().And(github =>
            {
                github.Kind.ShouldBe(GitHubWorkItemKind.Discussion);
            });
        }

        [Fact]
        public void Should_Map_Tags_Correctly()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Issue);
            data.WorkItem.Tags.Add(new WorkItemTagData
            {
                Tag = new TagData
                {
                    Id = 10,
                    Discriminator = Discriminator.GitHub,
                    GitHubId = 11,
                    Name = "bug",
                    Description = "A bug",
                    Color = "#ff0000",
                },
            });

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.ShouldBeOfType<GitHubNotification>().And(github =>
            {
                github.Tags.Count.ShouldBe(1);
                github.Tags[0].ShouldBeOfType<GitHubLabel>().And(tag =>
                {
                    tag.Id.ShouldBe(10);
                    tag.GitHubId.ShouldBe(11);
                    tag.Name.ShouldBe("bug");
                    tag.Description.ShouldBe("A bug");
                    tag.Color.ShouldBe("#ff0000");
                });
            });
        }

        [Fact]
        public void Should_Map_Milestones_Correctly()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Issue);
            data.WorkItem.Milestone = new MilestoneData
            {
                Id = 97,
                GitHubId = 98,
                Name = "v1.0.1",
            };

            // When
            var result = NotificationMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.ShouldBeOfType<GitHubNotification>().And(github =>
            {
                github.Milestone.ShouldNotBeNull();
                github.Milestone.ShouldBeOfType<GitHubMilestone>().And(milestone =>
                {
                    milestone.Id.ShouldBe(97);
                    milestone.GitHubId.ShouldBe(98);
                    milestone.Name.ShouldBe("v1.0.1");
                });
            });
        }

        public sealed class Fixture
        {
            public static NotificationData GetData(GitHubWorkItemKind kind)
            {
                return new NotificationData
                {
                    Id = 1,
                    Discriminator = Discriminator.GitHub,
                    Category = new CategoryData
                    {
                        Id = 22,
                    },
                    AccountId = 2,
                    Starred = true,
                    Muted = false,
                    Unread = true,
                    GitHubId = 3,
                    Reason = "Foo",
                    Timestamp = new DateTime(2015, 8, 25),
                    WorkItem = new WorkItemData()
                    {
                        Id = 4,
                        Discriminator = Discriminator.GitHub,
                        IsPullRequest = kind == GitHubWorkItemKind.PullRequest,
                        IsIssue = kind == GitHubWorkItemKind.Issue,
                        IsRelease = kind == GitHubWorkItemKind.Release,
                        IsVulnerability = kind == GitHubWorkItemKind.Vulnerability,
                        IsCommit = kind == GitHubWorkItemKind.Commit,
                        IsDiscussion = kind == GitHubWorkItemKind.Discussion,
                        GitHubLocalId = 5,
                        Merged = true,
                        Locked = true,
                        Url = "https://github.com/cake-build/cake/issues/2531",
                        Preamble = "Bar",
                        Title = "Baz",
                        IsClosed = true,
                        IsDraft = true,
                        Repository = new RepositoryData
                        {
                            Discriminator = Discriminator.GitHub,
                            Owner = "cake-build",
                            Name = "cake",
                        },
                        Tags = new List<WorkItemTagData>(),
                    },
                };
            }
        }
    }
}

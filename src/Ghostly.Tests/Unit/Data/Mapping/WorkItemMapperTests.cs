using System;
using System.Collections.Generic;
using Ghostly.Data.Mapping;
using Ghostly.Data.Models;
using Ghostly.Domain.GitHub;
using Ghostly.Testing;
using Shouldly;
using Xunit;

namespace Ghostly.Tests.Unit.Data.Mapping
{
    public sealed class WorkItemMapperTests
    {
        [Fact]
        public void Should_Map_GitHub_Issue_Data_To_GitHub_Issue()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Issue);

            // When
            var result = WorkItemMapper.Map(data);

            // Then
            result.ShouldBeOfType<GitHubWorkItem>().And(workitem =>
            {
                workitem.Id.ShouldBe(1);
                workitem.State.ShouldBe(WorkItemState.Reopened);
                workitem.Tags.ShouldNotBeNull();

                workitem.GitHubId.ShouldBe(2);
                workitem.Url.ShouldBe(new Uri("https://github.com/cake-build/cake/issues/2532", UriKind.RelativeOrAbsolute));
                workitem.Title.ShouldBe("Foo");
                workitem.Preamble.ShouldBe("Bar");
                workitem.Body.ShouldBe("Baz");

                workitem.CreatedAt.ShouldBe(new DateTime(2015, 8, 25));
                workitem.UpdatedAt.ShouldBe(new DateTime(2016, 8, 25));

                workitem.Repository.Id.ShouldBe(7);
                workitem.Repository.GitHubId.ShouldBe(4004);
                workitem.Repository.Owner.ShouldBe("cake-build");
                workitem.Repository.Name.ShouldBe("cake");
                workitem.Origin.ShouldBe("cake-build/cake");

                workitem.Merged.ShouldBe(true);
                workitem.MergedAt.ShouldBe(new DateTime(2017, 1, 12));

                workitem.MergedBy.ShouldBe(null);
                workitem.Locked.ShouldBe(true);

                workitem.Author.ShouldBeOfType<GitHubUser>().And(user =>
                {
                    user.Id.ShouldBe(1);
                    user.GitHubId.ShouldBe(2001);
                    user.AvatarUrl.ShouldBe("https://avatars3.githubusercontent.com/u/357872?s=460&v=4");
                    user.Username.ShouldBe("patriksvensson");
                    user.Name.ShouldBe("Patrik Svensson");
                });

                workitem.Milestone.ShouldBeOfType<GitHubMilestone>().And(milestone =>
                {
                    milestone.Id.ShouldBe(99);
                    milestone.GitHubId.ShouldBe(100);
                    milestone.Name.ShouldBe("foo");
                });

                workitem.Comments.ShouldNotBeNull();
                workitem.Comments.Count.ShouldBe(2);
                workitem.Comments[0].ShouldBeOfType<GitHubComment>().And(comment =>
                {
                    comment.Id.ShouldBe(1);
                    comment.GitHubId.ShouldBe(1001);
                    comment.CreatedAt.ShouldBe(new DateTime(2017, 8, 25));
                    comment.UpdatedAt.ShouldBe(new DateTime(2017, 8, 26));
                    comment.Url.AbsoluteUri.ShouldBe("https://github.com/cake-build/cake/issues/2532#issuecomment-1");

                    comment.Author.ShouldNotBeNull();
                    comment.Author.ShouldBeOfType<GitHubUser>().And(user =>
                    {
                        user.Id.ShouldBe(1);
                        user.GitHubId.ShouldBe(2001);
                        user.AvatarUrl.ShouldBe("https://avatars3.githubusercontent.com/u/357872?s=460&v=4");
                        user.Username.ShouldBe("patriksvensson");
                        user.Name.ShouldBe("Patrik Svensson");
                    });
                });

                workitem.Comments[1].ShouldBeOfType<GitHubComment>().And(comment =>
                {
                    comment.Id.ShouldBe(2);
                    comment.GitHubId.ShouldBe(1002);
                    comment.CreatedAt.ShouldBe(new DateTime(2018, 8, 25));
                    comment.UpdatedAt.ShouldBe(new DateTime(2018, 8, 26));
                    comment.Url.AbsoluteUri.ShouldBe("https://github.com/cake-build/cake/issues/2532#issuecomment-2");

                    comment.Author.ShouldNotBeNull();
                    comment.Author.ShouldBeOfType<GitHubUser>().And(user =>
                    {
                        user.Id.ShouldBe(2);
                        user.GitHubId.ShouldBe(2002);
                        user.AvatarUrl.ShouldBe("https://avatars1.githubusercontent.com/u/1271146?s=460&v=4");
                        user.Username.ShouldBe("gep13");
                        user.Name.ShouldBe("Gary Ewan Park");
                    });
                });
            });
        }

        [Fact]
        public void Should_Map_Commit_Information()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.Commit);
            data.CommitSha = "4ee26eaa873718e51f9392cc2295b70a573b0001";

            // When
            var result = WorkItemMapper.Map(data);

            // Then
            result.ShouldBeOfType<GitHubWorkItem>().And(workitem =>
            {
                workitem.Kind.ShouldBe(GitHubWorkItemKind.Commit);
                workitem.CommitSha.ShouldBe("4ee26eaa873718e51f9392cc2295b70a573b0001");
            });
        }

        [Fact]
        public void Should_Map_MergedBy()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.PullRequest);
            data.MergedBy = new UserData
            {
                Id = 1,
                GitHubId = 123,
                Login = "patriksvensson",
                Name = "Patrik Svensson",
                Discriminator = Discriminator.GitHub,
                AvatarUrl = "https://avatar",
                AvatarHash = "abc123",
            };

            // When
            var result = WorkItemMapper.Map(data);

            // Then
            result.ShouldBeOfType<GitHubWorkItem>().And(workitem =>
            {
                workitem.MergedBy.ShouldBeOfType<GitHubUser>().And(user =>
                {
                    user.Id.ShouldBe(1);
                    user.GitHubId.ShouldBe(123);
                    user.Username.ShouldBe("patriksvensson");
                    user.Name.ShouldBe("Patrik Svensson");
                    user.AvatarUrl.ShouldBe("https://avatar");
                    user.AvatarHash.ShouldBe("abc123");
                });
            });
        }

        [Fact]
        public void Should_Map_Draft_State()
        {
            // Given
            var data = Fixture.GetData(GitHubWorkItemKind.PullRequest);

            // When
            var result = WorkItemMapper.Map(data);

            // Then
            result.ShouldBeOfType<GitHubWorkItem>().And(workitem =>
            {
                workitem.IsDraft.ShouldNotBeNull();
                workitem.IsDraft.Value.ShouldBeTrue();
            });
        }

        private static class Fixture
        {
            public static WorkItemData GetData(GitHubWorkItemKind kind)
            {
                var data = new WorkItemData
                {
                    Id = 1,
                    Discriminator = Discriminator.GitHub,
                    IsPullRequest = kind == GitHubWorkItemKind.PullRequest,
                    IsIssue = kind == GitHubWorkItemKind.Issue,
                    IsRelease = kind == GitHubWorkItemKind.Release,
                    IsVulnerability = kind == GitHubWorkItemKind.Vulnerability,
                    IsCommit = kind == GitHubWorkItemKind.Commit,
                    IsReopened = true,
                    IsDraft = true,
                    Merged = true,
                    Locked = true,
                    MergedAt = new DateTime(2017, 1, 12),
                    Tags = new List<WorkItemTagData>
                    {
                        new WorkItemTagData
                        {
                            Tag = new TagData
                            {
                                Id = 1,
                                Discriminator = Discriminator.GitHub,
                                Name = "bug",
                                GitHubId = 3001,
                                Color = "#ff0000",
                                Description = "A bug",
                            },
                        },
                    },

                    GitHubId = 2,
                    GitHubLocalId = 3,
                    Url = "https://github.com/cake-build/cake/issues/2532",
                    Title = "Foo",
                    Preamble = "Bar",
                    Body = "Baz",

                    Repository = new RepositoryData
                    {
                        Id = 8,
                        Discriminator = Discriminator.GitHub,
                        GitHubId = 3003,
                        Owner = "cake-build",
                        Name = "cake",
                    },

                    Author = new UserData
                    {
                        Id = 1,
                        Discriminator = Discriminator.GitHub,
                        GitHubId = 2001,
                        AvatarUrl = "https://avatars3.githubusercontent.com/u/357872?s=460&v=4",
                        Login = "patriksvensson",
                        Name = "Patrik Svensson",
                    },

                    Milestone = new MilestoneData
                    {
                        Id = 99,
                        GitHubId = 100,
                        Name = "foo",
                    },
                };

                data.Repository = new RepositoryData()
                {
                    Id = 7,
                    Discriminator = Discriminator.GitHub,
                    GitHubId = 4004,
                    Owner = "cake-build",
                    Name = "cake",
                };

                data.CreatedAt = new DateTime(2015, 8, 25);
                data.UpdatedAt = new DateTime(2016, 8, 25);

                data.Comments = new List<CommentData>
                {
                    new CommentData
                    {
                        Id = 1,
                        Discriminator = Discriminator.GitHub,
                        GitHubId = 1001,
                        Author = new UserData
                        {
                            Id = 1,
                            Discriminator = Discriminator.GitHub,
                            GitHubId = 2001,
                            AvatarUrl = "https://avatars3.githubusercontent.com/u/357872?s=460&v=4",
                            Login = "patriksvensson",
                            Name = "Patrik Svensson",
                        },
                        Body = "Hello",
                        CreatedAt = new DateTime(2017, 8, 25),
                        UpdatedAt = new DateTime(2017, 8, 26),
                        Url = "https://github.com/cake-build/cake/issues/2532#issuecomment-1",
                    },
                    new CommentData
                    {
                        Id = 2,
                        Discriminator = Discriminator.GitHub,
                        GitHubId = 1002,
                        Author = new UserData
                        {
                            Id = 2,
                            Discriminator = Discriminator.GitHub,
                            GitHubId = 2002,
                            AvatarUrl = "https://avatars1.githubusercontent.com/u/1271146?s=460&v=4",
                            Login = "gep13",
                            Name = "Gary Ewan Park",
                        },
                        Body = "Hello",
                        CreatedAt = new DateTime(2018, 8, 25),
                        UpdatedAt = new DateTime(2018, 8, 26),
                        Url = "https://github.com/cake-build/cake/issues/2532#issuecomment-2",
                    },
                };

                return data;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.GitHub;
using Ghostly.GitHub.Actions;
using Ghostly.GitHub.Octokit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Ghostly.Tests.Unit.GitHub
{
    public sealed class UpdateGitHubWorkItemLabelsTests
    {
        [Fact]
        public async Task Should_Remove_Local_Labels_That_Have_Been_Remotely_Removed()
        {
            // Given
            var fixture = new Fixture();

            var labels = Array.Empty<GitHubLabelInfo>();
            var workitem = new WorkItemData()
            {
                Discriminator = Discriminator.GitHub,
                IsIssue = true,
                Tags = new List<WorkItemTagData>()
                {
                    new WorkItemTagData
                    {
                        TagId = 1,
                        Tag = new TagData
                        {
                            Id = 1,
                            Discriminator = Discriminator.GitHub,
                            GitHubId = 9999,
                            Name = "foo",
                        },
                    },
                },
            };

            // When
            await fixture.Handle(workitem, labels);

            // Then
            workitem.Tags.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Should_Not_Do_Anything_If_There_Are_Neither_Local_Or_Remote_Labels()
        {
            // Given
            var fixture = new Fixture();

            var labels = Array.Empty<GitHubLabelInfo>();
            var workitem = new WorkItemData() { Discriminator = Discriminator.GitHub, IsIssue = true };

            // When
            await fixture.Handle(workitem, labels);

            // Then
            workitem.Tags.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Add_New_Labels()
        {
            // Given
            var fixture = new Fixture();

            var labels = new GitHubLabelInfo[] { new GitHubLabelInfo { Id = 9999 } };
            var workitem = new WorkItemData() { Discriminator = Discriminator.GitHub, IsIssue = true };

            // When
            await fixture.Handle(workitem, labels);

            // Then
            workitem.Tags.Count.ShouldBe(1);
        }

        internal sealed class Fixture
        {
            public IMediator Mediator { get; set; }
            public IGhostlyLog Log { get; set; }
            public IGitHubGateway GitHub { get; set; }
            public UpdateGitHubWorkItemLabels Handler { get; set; }
            public GhostlyContext Context { get; set; }

            public Fixture()
            {
                Mediator = Substitute.For<IMediator>();
                Log = Substitute.For<IGhostlyLog>();
                GitHub = Substitute.For<IGitHubGateway>();
                Handler = new UpdateGitHubWorkItemLabels(Log);

                Context = new GhostlyContext(new DbContextOptionsBuilder<GhostlyContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)).Options);
            }

            public async Task Handle(WorkItemData workitem, IEnumerable<GitHubLabelInfo> labels)
            {
                await Handler.Handle(
                    new UpdateGitHubWorkItemLabels.Request(
                        GitHub, Context, workitem, labels), CancellationToken.None);
            }
        }
    }
}

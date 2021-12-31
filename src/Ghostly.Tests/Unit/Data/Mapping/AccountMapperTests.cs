using System;
using Ghostly.Core.Services;
using Ghostly.Data.Mapping;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;
using Ghostly.Testing;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Ghostly.Tests.Unit.Data.Mapping
{
    public sealed class AccountMapperTests
    {
        [Fact]
        public void Should_Map_GitHub_Account_Data_To_GitHub_Account()
        {
            // Given
            var data = new AccountData
            {
                Id = 1,
                Discriminator = Discriminator.GitHub,
                LastSyncedAt = new DateTime(2015, 8, 25),
                State = AccountState.Deleted,
                AvatarUrl = "https://avatars3.githubusercontent.com/u/357872?s=460&v=4",
                Scopes = "repo;notifications",
                Username = "patriksvensson",
            };

            // When
            var result = AccountMapper.Map(data, Substitute.For<ILocalizer>());

            // Then
            result.ShouldBeOfType<GitHubAccount>().And(github =>
            {
                github.Id.ShouldBe(1);
                github.LastSyncedAt.ShouldBe(new DateTime(2015, 8, 25));
                github.State.ShouldBe(AccountState.Deleted);
                github.AvatarUrl.ShouldBe("https://avatars3.githubusercontent.com/u/357872?s=460&v=4");
                github.Scopes.ShouldContain("repo");
                github.Scopes.ShouldContain("notifications");
                github.Username.ShouldBe("patriksvensson");
                github.VendorKind.ShouldBe(Vendor.GitHub);
            });
        }

        [Fact]
        public void Should_Map_GitHub_Account_To_GitHub_Account_Data()
        {
            // Given
            var data = new GitHubAccount
            {
                Id = 1,
                LastSyncedAt = new DateTime(2015, 8, 25),
                State = AccountState.Deleted,
                AvatarUrl = "https://avatars3.githubusercontent.com/u/357872?s=460&v=4",
                Scopes = new string[] { "repo", "notifications" },
                Username = "patriksvensson",
            };

            // When
            var result = AccountMapper.Map(data);

            // Then
            result.ShouldBeOfType<AccountData>().And(github =>
            {
                github.Id.ShouldBe(1);
                github.Discriminator.ShouldBe(Discriminator.GitHub);
                github.LastSyncedAt.ShouldBe(new DateTime(2015, 8, 25));
                github.State.ShouldBe(AccountState.Deleted);
                github.AvatarUrl.ShouldBe("https://avatars3.githubusercontent.com/u/357872?s=460&v=4");
                github.Scopes.ShouldBe("repo;notifications");
                github.Username.ShouldBe("patriksvensson");
            });
        }
    }
}

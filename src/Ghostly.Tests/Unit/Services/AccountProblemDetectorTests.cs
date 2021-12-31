using System;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Domain.Messages;
using Ghostly.Features.Accounts;
using Ghostly.Services;
using Ghostly.Testing;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Ghostly.Tests.Unit.Services
{
    public sealed class AccountProblemDetectorTests
    {
        [Fact]
        public async Task Should_Publish_Message_If_Account_Problem_Occur()
        {
            // Given
            var fixture = new Fixture(nameof(Should_Publish_Message_If_Account_Problem_Occur));
            await fixture.Initialize(async context =>
            {
                context.Accounts.Add(new AccountData { Id = 1, State = AccountState.Active });
                await context.SaveChangesAsync();
            });

            // When
            await fixture.UpdateAccountState(1, AccountState.Unauthorized);

            // Then
            fixture.DetectCount.ShouldBe(1);
        }

        [Fact]
        public async Task Should_Not_Publish_Message_If_Account_Problem_Is_Known()
        {
            // Given
            var fixture = new Fixture(nameof(Should_Not_Publish_Message_If_Account_Problem_Is_Known));
            await fixture.Initialize(async context =>
            {
                context.Accounts.Add(new AccountData { Id = 1, State = AccountState.Active });
                await context.SaveChangesAsync();
            });

            // When
            await fixture.UpdateAccountState(1, AccountState.Unauthorized);
            await fixture.UpdateAccountState(1, AccountState.Unauthorized);

            // Then
            fixture.DetectCount.ShouldBe(1);
        }

        [Fact]
        public async Task Should_Publish_Message_If_Account_Problem_Is_Resolved()
        {
            // Given
            var fixture = new Fixture(nameof(Should_Publish_Message_If_Account_Problem_Is_Resolved));
            await fixture.Initialize(async context =>
            {
                context.Accounts.Add(new AccountData { Id = 1, State = AccountState.Unauthorized });
                await context.SaveChangesAsync();
            });

            // When
            await fixture.UpdateAccountState(1, AccountState.Active);

            // Then
            fixture.ResolveCount.ShouldBe(1);
        }

        public sealed class Fixture
        {
            public FakeGhostlyContextFactory Factory { get; }
            public MessageService Messenger { get; }
            public AccountProblemDetector Detector { get; set; }
            public UpdateAccountStateHandler AccountStateUpdater { get; set; }

            public int DetectCount { get; private set; }
            public int ResolveCount { get; private set; }

            public Fixture(string name)
            {
                Factory = new FakeGhostlyContextFactory(name);
                Messenger = new MessageService(Substitute.For<IGhostlyLog>());
                Detector = new AccountProblemDetector(Factory, Messenger);
                AccountStateUpdater = new UpdateAccountStateHandler(Factory, Messenger);

                Messenger.SubscribeAsync<AccountProblemDetected>(m =>
                {
                    DetectCount++;
                    return Task.CompletedTask;
                });

                Messenger.SubscribeAsync<AccountProblemResolved>(m =>
                {
                    ResolveCount++;
                    return Task.CompletedTask;
                });
            }

            public async Task Initialize(Func<GhostlyContext, Task> action = null)
            {
                await action?.Invoke(Factory.Create());
                await Detector.Initialize(false);
            }

            public async Task UpdateAccountState(int accountId, AccountState state)
            {
                await AccountStateUpdater.Handle(
                    new UpdateAccountStateHandler.Request(accountId, state),
                    default);
            }
        }
    }
}

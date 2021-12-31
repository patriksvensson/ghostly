using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Services;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Ghostly.Tests.Unit.Services
{
    public sealed class MessageServiceTests
    {
        [Fact]
        public async Task Should_Receive_Subscribed_Message_Types()
        {
            using (var service = new MessageService(Substitute.For<IGhostlyLog>()))
            {
                // Given
                var inbox = new List<string>();
                service.SubscribeAsync<string>(message =>
                {
                    inbox.Add(message);
                    return Task.CompletedTask;
                });

                // When
                await service.Publish("Hello World!", m => m());

                // Then
                inbox.Count.ShouldBe(1);
                inbox[0].ShouldBe("Hello World!");
            }
        }

        [Fact]
        public async Task Should_Not_Register_Same_Handler_Twice()
        {
            using (var service = new MessageService(Substitute.For<IGhostlyLog>()))
            {
                // Given
                var inbox = new List<string>();
                var handler = (Func<string, Task>)(m =>
                {
                    inbox.Add(m);
                    return Task.CompletedTask;
                });

                service.SubscribeAsync(handler);
                service.SubscribeAsync(handler);

                // When
                await service.Publish("Hello World!", m => m());

                // Then
                inbox.Count.ShouldBe(1);
                inbox[0].ShouldBe("Hello World!");
            }
        }
    }
}

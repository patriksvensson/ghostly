using Ghostly.Data.Mapping;
using Ghostly.Data.Models;
using Shouldly;
using Xunit;

namespace Ghostly.Tests.Unit.Data.Mapping
{
    public sealed class RepositoryMapperTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_Map_Private_State(bool isPrivate)
        {
            // Given
            var data = new RepositoryData()
            {
                GitHubId = 1,
                Private = isPrivate,
            };

            // When
            var result = RepositoryMapper.Map(data);

            // Then
            result.Private.ShouldBe(isPrivate);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_Map_Fork_State(bool isFork)
        {
            // Given
            var data = new RepositoryData()
            {
                GitHubId = 1,
                Fork = isFork,
            };

            // When
            var result = RepositoryMapper.Map(data);

            // Then
            result.Fork.ShouldBe(isFork);
        }
    }
}

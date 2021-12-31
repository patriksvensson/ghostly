using Shouldly;
using Xunit;

namespace Ghostly.Core.Tests.Unit.Extensions
{
    public sealed class StringExtensionsTests
    {
        public sealed class TheGetWordAtPositionMethod
        {
            [Theory]
            [InlineData("Foo Bar", 0, "F")]
            [InlineData("Foo Bar", 1, "Fo")]
            [InlineData("Foo Bar", 2, "Foo")]
            [InlineData("Foo Bar", 4, "B")]
            [InlineData("Foo Bar", 5, "Ba")]
            [InlineData("Foo Bar", 6, "Bar")]
            public void Should_Return_Expected_Word(string text, int position, string expected)
            {
                // Given, When
                var result = text.GetPartialWordAtPosition(position);

                // Then
                result.ShouldBe(expected);
            }

            [Theory]
            [InlineData("Foo Bar", 3, "")]
            public void Should_Return_Empty_String_If_Position_Is_At_Space(string text, int position, string expected)
            {
                // Given, When
                var result = text.GetPartialWordAtPosition(position);

                // Then
                result.ShouldBe(expected);
            }

            [Theory]
            [InlineData("Foo Bar", 7, "")]
            [InlineData("Foo Bar", 9, "")]
            [InlineData("Foo Bar", -1, "")]
            [InlineData("Foo Bar", -7, "")]
            public void Should_Return_Empty_String_If_Position_Is_Out_Of_Bounds(string text, int position, string expected)
            {
                // Given, When
                var result = text.GetPartialWordAtPosition(position);

                // Then
                result.ShouldBe(expected);
            }
        }
    }
}

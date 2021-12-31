using Shouldly;
using Xunit;

namespace Ghostly.Core.Tests.Unit
{
    public sealed class ColorRepresentationTests
    {
        public sealed class TheParseHexMethod
        {
            [Theory]
            [InlineData("", new byte[] { 255, 255, 255, 255 })]
            [InlineData("#", new byte[] { 255, 255, 255, 255 })]
            [InlineData("#CCFFEEDD", new byte[] { 204, 255, 238, 221 })]
            [InlineData("#ccffeedd", new byte[] { 204, 255, 238, 221 })]
            [InlineData("CCFFEEDD", new byte[] { 204, 255, 238, 221 })]
            [InlineData("ccffeedd", new byte[] { 204, 255, 238, 221 })]
            [InlineData("#FFEEDD", new byte[] { 255, 255, 238, 221 })]
            [InlineData("#ffeedd", new byte[] { 255, 255, 238, 221 })]
            [InlineData("FFEEDD", new byte[] { 255, 255, 238, 221 })]
            [InlineData("ffeedd", new byte[] { 255, 255, 238, 221 })]
            [InlineData("#FED", new byte[] { 255, 255, 238, 221 })]
            [InlineData("#fed", new byte[] { 255, 255, 238, 221 })]
            [InlineData("FED", new byte[] { 255, 255, 238, 221 })]
            [InlineData("fed", new byte[] { 255, 255, 238, 221 })]
            [InlineData("#CFED", new byte[] { 204, 255, 238, 221 })]
            [InlineData("#cfed", new byte[] { 204, 255, 238, 221 })]
            [InlineData("CFED", new byte[] { 204, 255, 238, 221 })]
            [InlineData("cfed", new byte[] { 204, 255, 238, 221 })]
            public void Should_Parse_Color(string input, byte[] expected)
            {
                // Given, When
                var components = ColorRepresentation.ParseHex(input);

                // Then
                components.Alpha.ShouldBe(expected[0]);
                components.Red.ShouldBe(expected[1]);
                components.Green.ShouldBe(expected[2]);
                components.Blue.ShouldBe(expected[3]);
            }
        }

        public sealed class TheToHexMethod
        {
            [Theory]
            [InlineData(new byte[] { 255, 238, 221, 204 }, "#FFEEDDCC")]
            public void Should_Convert_Color_With_Four_Components_To_Hex(byte[] colors, string expected)
            {
                // Given
                var color = new ColorRepresentation(colors[0], colors[1], colors[2], colors[3]);

                // When
                var result = color.ToHex();

                // Then
                result.ShouldBe(expected);
            }
        }
    }
}

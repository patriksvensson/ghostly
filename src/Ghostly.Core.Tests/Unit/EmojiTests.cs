using Ghostly.Core.Text;
using Shouldly;
using Xunit;

namespace Ghostly.Core.Tests.Unit
{
    public sealed class EmojiTests
    {
        [Fact]
        public void Should_Convert_Supported_Emojis_To_Text_Representation_Correctly()
        {
            foreach (var emoji in Emoji.Supported)
            {
                var shortcode = EmojiHelper.GetEmojiShortcode(emoji.Value);
                shortcode.ShouldNotBeNull($"No shortcode found for emoji '{emoji.Value}' ({emoji.Name})");
            }
        }

        [Fact]
        public void Should_Convert_Supported_Emoji_Text_Representation_Back_To_Text_Correctly()
        {
            foreach (var emoji in Emoji.Supported)
            {
                var shortcode = EmojiHelper.GetEmojiShortcode(emoji.Value);
                shortcode.ShouldNotBeNull($"No shortcode found for emoji '{emoji.Value}' ({emoji.Name})");

                var result = EmojiHelper.GetEmoji(shortcode);
                result.ShouldNotBeNull($"Could not convert shortcode {shortcode} to emoji");
                result.ShouldBe(emoji.Value, $"Converted emoji {shortcode} does not represent original");
            }
        }

        [Fact]
        public void Should_Replace_Emoji_In_Text()
        {
            // Given, When
            var result = EmojiHelper.Replace("Hello :imp: world! Do you want some :cake:?");

            // Then
            result.ShouldBe("Hello 👿 world! Do you want some 🍰?");
        }
    }
}

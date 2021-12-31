using System.Collections.Generic;
using System.Linq;
using Ghostly.Features.Querying;
using Shouldly;
using Xunit;

namespace Ghostly.Tests.Unit.Features.Querying
{
    public sealed class AutoCompleteStateTests
    {
        public sealed class TheTryPopulateSuggestionsMethod
        {
            [Fact]
            public void Should_Populate_Suggested_Items_For_Partial_Match()
            {
                // Given
                var state = new AutoCompleteState(
                    () => null,
                    new List<AutoCompleteItem>
                    {
                        new AutoCompleteItem { Name = "foo" },
                        new AutoCompleteItem { Name = "bar" },
                        new AutoCompleteItem { Name = "baz" },
                        new AutoCompleteItem { Name = "qux" },
                    });

                state.UpdateText("b", 1);

                // When
                var result = state.TryPopulateSuggestions(false, out var _);

                // Then
                result.ShouldBeTrue();
                state.Suggestions.Count.ShouldBe(2);
                state.Suggestions.ElementAt(0).Name.ShouldBe("bar");
                state.Suggestions.ElementAt(1).Name.ShouldBe("baz");
            }

            [Fact]
            public void Should_Populate_Suggested_Items_After_Colon()
            {
                // Given
                var state = new AutoCompleteState(
                    () => null,
                    new List<AutoCompleteItem>
                    {
                        new AutoCompleteItem { Name = "foo", ReturnType = typeof(bool) },
                        new AutoCompleteItem { Name = "bar", ReturnType = typeof(bool) },
                        new AutoCompleteItem { Name = "baz", ReturnType = typeof(bool) },
                        new AutoCompleteItem { Name = "qux", ReturnType = typeof(bool) },
                    });

                state.UpdateText("is:", 3);

                // When
                var result = state.TryPopulateSuggestions(false, out var _);

                // Then
                result.ShouldBeTrue();
                state.Suggestions.Count.ShouldBe(4);
            }

            [Fact]
            public void Should_Populate_Suggested_Items_With_Symbol_After_Symbol()
            {
                // Given
                var state = new AutoCompleteState(
                    () => null,
                    new List<AutoCompleteItem>
                    {
                        new AutoCompleteItem { Name = "!", ReturnType = typeof(bool) },
                        new AutoCompleteItem { Name = "!=", ReturnType = typeof(bool) },
                    });

                state.UpdateText("!", 1);

                // When
                var result = state.TryPopulateSuggestions(false, out var _);

                // Then
                result.ShouldBeTrue();
                state.Suggestions.Count.ShouldBe(2);
            }

            [Fact]
            public void Should_Not_Populate_Suggested_Items_With_Symbol_After_Matched_Symbol()
            {
                // Given
                var state = new AutoCompleteState(
                    () => null,
                    new List<AutoCompleteItem>
                    {
                        new AutoCompleteItem { Name = "!" },
                        new AutoCompleteItem { Name = "!=" },
                        new AutoCompleteItem { Name = "==" },
                    });

                state.UpdateText("!=", 2);

                // When
                var result = state.TryPopulateSuggestions(false, out var _);

                // Then
                result.ShouldBeFalse();
            }

            [Fact]
            public void Should_Populate_Suggested_Items_With_Word_After_Matched_Symbol()
            {
                // Given
                var state = new AutoCompleteState(
                    () => null,
                    new List<AutoCompleteItem>
                    {
                        new AutoCompleteItem { Name = "!" },
                        new AutoCompleteItem { Name = "!=" },
                        new AutoCompleteItem { Name = "==" },
                        new AutoCompleteItem { Name = "foo" },
                        new AutoCompleteItem { Name = "bar" },
                    });

                state.UpdateText("!=f", 3);

                // When
                var result = state.TryPopulateSuggestions(false, out var _);

                // Then
                result.ShouldBeTrue();
                state.Suggestions.Count.ShouldBe(1);
                state.Suggestions.ElementAt(0).Name.ShouldBe("foo");
            }

            [Fact]
            public void Should_Not_Populate_Suggested_Items_For_Exact_Match()
            {
                // Given
                var state = new AutoCompleteState(
                    () => null,
                    new List<AutoCompleteItem>
                    {
                        new AutoCompleteItem { Name = "foo" },
                        new AutoCompleteItem { Name = "bar" },
                        new AutoCompleteItem { Name = "baz" },
                        new AutoCompleteItem { Name = "qux" },
                    });

                state.UpdateText("bar", 3);

                // When
                var result = state.TryPopulateSuggestions(false, out var _);

                // Then
                result.ShouldBeFalse();
                state.Suggestions.Count.ShouldBe(0);
            }

            [Fact]
            public void Should_Populate_Suggested_Items_For_Partial_Match_When_Previous_Token_Is_A_Esclamation_Mark()
            {
                // Given
                var state = new AutoCompleteState(
                    () => null,
                    new List<AutoCompleteItem>
                    {
                        new AutoCompleteItem { Name = "foo" },
                        new AutoCompleteItem { Name = "bar" },
                        new AutoCompleteItem { Name = "baz" },
                        new AutoCompleteItem { Name = "qux" },
                    });

                state.UpdateText("!b", 2);

                // When
                var result = state.TryPopulateSuggestions(true, out var _);

                // Then
                result.ShouldBeTrue();
                state.Suggestions.Count.ShouldBe(2);
                state.Suggestions.ElementAt(0).Name.ShouldBe("bar");
                state.Suggestions.ElementAt(1).Name.ShouldBe("baz");
            }
        }

        public sealed class TheTryInsertItemMethod
        {
            [Theory]
            [InlineData("b", 1)]
            [InlineData("ba", 2)]
            [InlineData("baz", 3)]
            [InlineData("baz", 1)]
            [InlineData("baz", 2)]
            public void Should_Insert_Selected_Item(string text, int position)
            {
                // Given
                var state = new AutoCompleteState(
                    () => new AutoCompleteItem { Name = "baz" },
                    Enumerable.Empty<AutoCompleteItem>());

                state.UpdateText(text, position);

                // When
                var result = state.TryInsertSelectedItem(false, out var newText, out var newPosition);

                // Then
                result.ShouldBeTrue();
                newText.ShouldBe("baz");
                newPosition.ShouldBe(3);
            }

            [Theory]
            [InlineData("b", 1)]
            [InlineData("ba", 2)]
            [InlineData("bar", 3)]
            [InlineData("bar", 1)]
            [InlineData("bar", 2)]
            public void Should_Insert_First_Item_If_None_Is_Selected(string text, int position)
            {
                // Given
                var state = new AutoCompleteState(
                    () => null,
                    new List<AutoCompleteItem>
                    {
                        new AutoCompleteItem { Name = "foo" },
                        new AutoCompleteItem { Name = "bar" },
                        new AutoCompleteItem { Name = "bark" },
                        new AutoCompleteItem { Name = "qux" },
                    });

                state.UpdateText(text, position);
                state.TryPopulateSuggestions(false, out var _);

                // When
                var result = state.TryInsertSelectedItem(true, out var newText, out var newPosition);

                // Then
                result.ShouldBeTrue();
                newText.ShouldBe("bar");
                newPosition.ShouldBe(3);
            }

            [Fact]
            public void Should_Insert_Item_After_Exclamation_Mark()
            {
                // Given
                var state = new AutoCompleteState(
                    () => null,
                    new List<AutoCompleteItem>
                    {
                        new AutoCompleteItem { Name = "foo" },
                        new AutoCompleteItem { Name = "bar" },
                        new AutoCompleteItem { Name = "baz" },
                        new AutoCompleteItem { Name = "qux" },
                    });

                state.UpdateText("!b", 2);
                state.TryPopulateSuggestions(false, out var _);

                // When
                var result = state.TryInsertSelectedItem(true, out var newText, out var newPosition);

                // Then
                result.ShouldBeTrue();
                newText.ShouldBe("!bar");
                newPosition.ShouldBe(4);
            }

            [Fact]
            public void Should_Insert_Exclamation_Mark_Over_Existing_Exclamation_Mark()
            {
                // Given
                var state = new AutoCompleteState(
                    () => new AutoCompleteItem { Name = "!" },
                    Enumerable.Empty<AutoCompleteItem>());

                state.UpdateText("!", 1);

                // When
                var result = state.TryInsertSelectedItem(false, out var newText, out var newPosition);

                // Then
                result.ShouldBeTrue();
                newText.ShouldBe("!");
                newPosition.ShouldBe(1);
            }

            [Fact]
            public void Should_Insert_Multiple_Token_Symbol_Over_Existing_Symbol()
            {
                // Given
                var state = new AutoCompleteState(
                    () => new AutoCompleteItem { Name = "!=" },
                    Enumerable.Empty<AutoCompleteItem>());

                state.UpdateText("!", 1);

                // When
                var result = state.TryInsertSelectedItem(false, out var newText, out var newPosition);

                // Then
                result.ShouldBeTrue();
                newText.ShouldBe("!=");
                newPosition.ShouldBe(2);
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Ghostly.Data.Models;
using Ghostly.Features.Querying;
using Shouldly;
using Xunit;

namespace Ghostly.Tests.Unit.Features.Querying
{
    public sealed class GhostlyQueryLanguageTests
    {
        public sealed class TheTryParseMethod
        {
            [Theory]
            [InlineData("@inbox OR @archived", "Or(Equals(Property(inbox),Constant(True)),Equals(Property(archived),Constant(True)))")]
            [InlineData("@starred AND (@inbox OR @archived)", "And(Equals(Property(starred),Constant(True)),Or(Equals(Property(inbox),Constant(True)),Equals(Property(archived),Constant(True))))")]
            [InlineData("@repository:cake", "Contains(Property(repository),Constant('cake'))")]
            [InlineData("@repository=cake", "Equals(Property(repository),Constant('cake'))")]
            [InlineData("not @starred", "Not(Equals(Property(starred),Constant(True)))")]
            [InlineData("(@starred)", "Equals(Property(starred),Constant(True))")]
            [InlineData("!@org:bar", "Not(Contains(Property(org),Constant('bar')))")]
            [InlineData("!@org==bar", "Not(Equals(Property(org),Constant('bar')))")]
            [InlineData("@org==cake-build", "Equals(Property(org),Constant('cake-build'))")]
            [InlineData("@repo=xamarin.forms", "Equals(Property(repo),Constant('xamarin.forms'))")]
            [InlineData("is:unread", "Equals(Property(unread),Constant(True))")]
            [InlineData("@unread=yes", "Equals(Property(unread),Constant(True))")]
            [InlineData("@unread=no", "Equals(Property(unread),Constant(False))")]
            public void Should_Extrapolate_Expressions_Correctly(string input, string expected)
            {
                // Given, When
                var result = GhostlyQueryLanguage.TryParse(input, out var expression, out var error);

                // Then
                result.ShouldBeTrue(error);
                expression.ToString().ShouldBe(expected);
            }

            [Theory]
            [InlineData("A = 'b", "Syntax error: unexpected end of input, expected `'`.")]
            [InlineData("A or B) and C", "Syntax error (line 1, column 7): unexpected `)`.")]
            [InlineData("A lik3 C", "Syntax error (line 1, column 3): unexpected literal `lik3`.")]
            [InlineData("A > 1234f", "Syntax error (line 1, column 9): unexpected `f`, expected digit.")]
            public void PreciseErrorsAreReported(string input, string expectedMessage)
            {
                // Given, When
                var result = GhostlyQueryLanguage.TryParse(input, out var _, out var error);

                // Then
                result.ShouldBeFalse();
                error.ShouldBe(expectedMessage);
            }

            [Theory]
            [InlineData("@assigned=gep13", "Equals(Collection(assigned),Constant('gep13'))")]
            public void Should_Parse_Collection_Properties_Correctly(string input, string expected)
            {
                // Given, When
                var result = GhostlyQueryLanguage.TryParse(input, out var expression, out var error);

                // Then
                result.ShouldBeTrue(error);
                expression.ToString().ShouldBe(expected);
            }

            [Theory]
            [InlineData("\"foo\"", "Or(Or(Or(Or(Contains(Property(body),StringLiteral('foo')),Contains(Property(title),StringLiteral('foo'))),Contains(Property(owner),StringLiteral('foo'))),Contains(Property(repo),StringLiteral('foo'))),Contains(Collection(comment),StringLiteral('foo')))")]
            [InlineData("'foo'", "Or(Or(Or(Or(Contains(Property(body),StringLiteral('foo')),Contains(Property(title),StringLiteral('foo'))),Contains(Property(owner),StringLiteral('foo'))),Contains(Property(repo),StringLiteral('foo'))),Contains(Collection(comment),StringLiteral('foo')))")]
            public void Should_Parse_Literals_Correctly(string input, string expected)
            {
                // Given, When
                var result = GhostlyQueryLanguage.TryParse(input, out var expression, out _);

                // Then
                result.ShouldBeTrue();
                expression.ToString().ShouldBe(expected);
            }

            [Fact]
            public void Should_Treat_Unknown_Property_As_Free_Text_Search()
            {
                // Given, When
                var result = GhostlyQueryLanguage.TryParse("foo", out var expression, out _);

                // Then
                result.ShouldBeTrue();
                expression.ToString().ShouldBe("Or(Or(Or(Or(Contains(Property(body),StringLiteral('foo')),Contains(Property(title),StringLiteral('foo'))),Contains(Property(owner),StringLiteral('foo'))),Contains(Property(repo),StringLiteral('foo'))),Contains(Collection(comment),StringLiteral('foo')))");
            }
        }

        public sealed class TheTryCompileMethod
        {
            [Fact]
            public void Should_Return_Error_On_Unknown_Properties()
            {
                // Given, When
                var result = GhostlyQueryLanguage.TryCompile("@foo = true", out var _, out var error);

                // Then
                result.ShouldBeFalse();
                error.ShouldBe("Unknown property foo.");
            }

            [Theory]
            [InlineData("@muted = true")]
            [InlineData("muted = true")]
            [InlineData("@muted")]
            [InlineData("muted")]
            public void Should_Compile_Correct_Query_For_Muted_Property(string query)
            {
                // Given
                var list = new List<NotificationData>()
                {
                    new NotificationData { Id = 1, Muted = false },
                    new NotificationData { Id = 2, Muted = true },
                }.AsQueryable();

                // When
                GhostlyQueryLanguage.TryCompile(query, out var expression, out var _);
                var result = list.Where(expression).ToList();

                // Then
                result.Count.ShouldBe(1);
                result[0].Id.ShouldBe(2);
                result[0].Muted.ShouldBeTrue();
            }

            [Fact]
            public void Should_Compile_Correct_Query_For_Reason_Property()
            {
                // Given
                var list = new List<NotificationData>()
                {
                    new NotificationData { Id = 1, WorkItem = new WorkItemData { GitHubLocalId = 5 } },
                    new NotificationData { Id = 2, WorkItem = new WorkItemData { GitHubLocalId = 10 } },
                    new NotificationData { Id = 3, WorkItem = new WorkItemData { GitHubLocalId = 15 } },
                }.AsQueryable();

                // When
                GhostlyQueryLanguage.TryCompile("@id >= 10", out var expression, out var _);
                var result = list.Where(expression).ToList();

                // Then
                result.Count.ShouldBe(2);
                result[0].Id.ShouldBe(2);
                result[1].Id.ShouldBe(3);
            }
        }
    }
}

using System;
using System.Linq;
using Ghostly.Core.Collections;
using Ghostly.Testing;
using Shouldly;
using Xunit;

namespace Ghostly.Core.Tests.Unit.Collections
{
    public sealed class DirectedGraphTests
    {
        [Fact]
        public void Should_Add_Nodes()
        {
            // Given
            var graph = new DirectedGraph<string>();

            // When
            graph.Add("Foo");

            // Then
            graph.Nodes.Contains("Foo").ShouldBeTrue();
        }

        [Fact]
        public void Should_Connect_Nodes()
        {
            // Given
            var graph = new DirectedGraph<string>();

            // When
            graph.Connect("Foo", "Bar");

            // Then
            graph.Edges.Count.ShouldBe(1);
            graph.Edges.ElementAt(0).From.ShouldBe("Foo");
            graph.Edges.ElementAt(0).To.ShouldBe("Bar");
        }

        [Fact]
        public void Should_Add_Nodes_When_Connecting_If_Missing()
        {
            // Given
            var graph = new DirectedGraph<string>();

            // When
            graph.Connect("Foo", "Bar");

            // Then
            graph.Nodes.Count.ShouldBe(2);
            graph.Nodes.Contains("Foo").ShouldBeTrue();
            graph.Nodes.Contains("Bar").ShouldBeTrue();
        }

        [Fact]
        public void Should_Throw_If_Trying_To_Add_Reflexive_Edge()
        {
            // Given
            var graph = new DirectedGraph<string>();

            // When
            var result = Record.Exception(() => graph.Connect("Foo", "Foo"));

            // Then
            result.ShouldBeOfType<InvalidOperationException>()
                .And().Message.ShouldBe("Reflexive edges in graph are not allowed.");
        }

        [Fact]
        public void Should_Throw_If_Trying_To_Add_Unidirectional_Edge()
        {
            // Given
            var graph = new DirectedGraph<string>();
            graph.Connect("Foo", "Bar");

            // When
            var result = Record.Exception(() => graph.Connect("Bar", "Foo"));

            // Then
            result.ShouldBeOfType<InvalidOperationException>()
                .And().Message.ShouldBe("Unidirectional edges in graph are not allowed.");
        }

        [Fact]
        public void Should_Traverse_Nodes_In_Correct_Order()
        {
            // Given
            var graph = new DirectedGraph<string>();
            graph.Connect("A", "B");
            graph.Connect("B", "C");
            graph.Connect("D", "E");
            graph.Connect("F", "C");
            graph.Connect("F", "A");
            graph.Connect("E", "C");

            // When
            var result = graph.Traverse().ToList();

            // Then
            result[0].ShouldBe("F");
            result[1].ShouldBe("A");
            result[2].ShouldBe("B");
            result[3].ShouldBe("D");
            result[4].ShouldBe("E");
            result[5].ShouldBe("C");
        }
    }
}
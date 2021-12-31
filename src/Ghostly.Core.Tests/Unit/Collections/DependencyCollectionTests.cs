using System.Linq;
using Ghostly.Core.Collections;
using Shouldly;
using Xunit;

namespace Ghostly.Core.Tests.Unit.Collections
{
    public sealed class DependencyCollectionTests
    {
        private sealed class Fixture
        {
            public abstract class Base { }

            [DependentOn(typeof(F))]
            public sealed class A : Base { }

            [DependentOn(typeof(A))]
            public sealed class B : Base { }

            [DependentOn(typeof(B))]
            [DependentOn(typeof(F))]
            [DependentOn(typeof(E))]
            public sealed class C : Base { }

            public sealed class D : Base { }

            [DependentOn(typeof(D))]
            public sealed class E : Base { }

            public sealed class F : Base { }
        }

        [Fact]
        public void Should_Traverse_Items_In_Correct_Order()
        {
            // Given
            var graph = new DependencyCollection<Fixture.Base>(new Fixture.Base[]
            {
                new Fixture.A(), new Fixture.B(),
                new Fixture.C(), new Fixture.D(),
                new Fixture.E(), new Fixture.F(),
            });

            // When
            var result = graph.ToList();

            // Then
            result[0].ShouldBeOfType<Fixture.F>();
            result[1].ShouldBeOfType<Fixture.A>();
            result[2].ShouldBeOfType<Fixture.B>();
            result[3].ShouldBeOfType<Fixture.D>();
            result[4].ShouldBeOfType<Fixture.E>();
            result[5].ShouldBeOfType<Fixture.C>();
        }
    }
}
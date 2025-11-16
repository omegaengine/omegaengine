using System.Collections.Generic;
using FluentAssertions;
using OmegaEngine.Foundation.Geometry;
using Xunit;

namespace OmegaEngine;

public class DistanceComparerTest
{
    private record DummyPositionable : IPositionable
    {
        public DoubleVector3 Position { get; set; }
    }

    private readonly DummyPositionable
        root = new() { Position = new(1, 2, 3) },
        a = new() { Position = new(0, 2, 3) },
        b = new() { Position = new(1, 2, 0) },
        c = new() { Position = new(1, 0, 3) };

    [Fact]
    public void Sort()
    {
        var list = new List<DummyPositionable> {a, b, c};
        list.Sort(new DistanceComparer(root));

        list.Should().Equal(a, c, b);
    }

    [Fact]
    public void SortInverse()
    {
        var list = new List<DummyPositionable> {a, b, c};
        list.Sort(new DistanceComparer(root, inverse: true));

        list.Should().Equal(b, c, a);
    }
}

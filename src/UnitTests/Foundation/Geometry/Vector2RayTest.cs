using FluentAssertions;
using SlimDX;
using Xunit;

namespace OmegaEngine.Foundation.Geometry;

public class Vector2RayTests
{
    [Fact]
    public void PerpendicularDistance_PointOnRay_ReturnsZero()
    {
        var ray = new Vector2Ray(position: new Vector2(0, 0), direction: new Vector2(1, 0));
        var pointOnRay = new Vector2(5, 0);

        float distance = ray.PerpendicularDistance(pointOnRay);
        distance.Should().BeApproximately(0, precision: 0.0001f);
    }

    [Fact]
    public void PerpendicularDistance_PointOffsetPerpendicularly_ReturnsCorrectDistance()
    {
        var ray = new Vector2Ray(position: new Vector2(2, 0), direction: new Vector2(1, 0));
        var point = new Vector2(5, 4);

        float distance = ray.PerpendicularDistance(point);
        distance.Should().BeApproximately(4, precision: 0.0001f);
    }

    [Fact]
    public void PerpendicularDistance_PointBehindRay_StillComputesPerpendicularDistance()
    {
        var ray = new Vector2Ray(position: new Vector2(0, 2), direction: new Vector2(1, 0));
        var point = new Vector2(-2, 5);

        float distance = ray.PerpendicularDistance(point);
        distance.Should().BeApproximately(3, precision: 0.0001f);
    }
}

using FluentAssertions;
using SlimDX;
using Xunit;

namespace OmegaEngine.Foundation.Geometry;

public class Vector3RayTests
{
    [Fact]
    public void PerpendicularDistance_PointOnRay_ReturnsZero()
    {
        var ray = new Vector3Ray(position: new Vector3(2, 0, 0), direction: new Vector3(1, 0, 0));
        var pointOnRay = new Vector3(7, 0, 0);

        float distance = ray.PerpendicularDistance(pointOnRay);
        distance.Should().BeApproximately(0, precision: 0.0001f);
    }

    [Fact]
    public void PerpendicularDistance_PointOffsetPerpendicularly_ReturnsCorrectDistance()
    {
        var ray = new Vector3Ray(position: new Vector3(0, 2, 0), direction: new Vector3(1, 0, 0));
        var point = new Vector3(3, 6, 0);

        float distance = ray.PerpendicularDistance(point);
        distance.Should().BeApproximately(4, precision: 0.0001f);
    }

    [Fact]
    public void PerpendicularDistance_PointBehindRay_StillComputesPerpendicularDistance()
    {
        var ray = new Vector3Ray(position: new Vector3(0, 0, 2), direction: new Vector3(1, 0, 0));
        var point = new Vector3(-2, 3, 2);

        float distance = ray.PerpendicularDistance(point);
        distance.Should().BeApproximately(3, precision: 0.0001f);
    }
}

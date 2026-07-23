using AlphaFramework.World.Terrains;
using FrameOfReference.World.Positionables;
using FrameOfReference.World.Templates;
using FluentAssertions;
using Xunit;

namespace FrameOfReference.World;

public class SessionTest
{
    [Fact]
    public void TestUpdateWithoutBurnChangeUsesConfiguredTimeWarp()
    {
        var session = new Session(new Universe(new Terrain<TerrainTemplate>(new TerrainSize(x: 3, y: 3))))
        {
            TimeWarpFactor = 100
        };

        session.Update(elapsedRealTime: 1).Should().Be(100);
    }

    [Fact]
    public void TestUpdateAtBurnChangeUsesDefaultTimeWarp()
    {
        var universe = new Universe(new Terrain<TerrainTemplate>(new TerrainSize(x: 3, y: 3)))
        {
            GameTime = 100
        };
        universe.Positionables.Add(new Entity
        {
            IsPlayerControlled = true,
            Waypoints =
            {
                new() {ActivationTime = 100}
            }
        });
        var session = new Session(universe) {TimeWarpFactor = 100};

        session.Update(elapsedRealTime: 1).Should().Be(10);
    }

    [Fact]
    public void TestUpdateNearBurnChangeRampsTimeWarp()
    {
        var universe = new Universe(new Terrain<TerrainTemplate>(new TerrainSize(x: 3, y: 3)))
        {
            GameTime = 90
        };
        universe.Positionables.Add(new Entity
        {
            IsPlayerControlled = true,
            Waypoints =
            {
                new() {ActivationTime = 100}
            }
        });
        var session = new Session(universe) {TimeWarpFactor = 100};

        session.Update(elapsedRealTime: 1).Should().Be(55);
    }
}

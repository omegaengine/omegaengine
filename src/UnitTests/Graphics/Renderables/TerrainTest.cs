/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using AwesomeAssertions;
using OmegaEngine.Foundation.Geometry;
using SlimDX;
using Xunit;

namespace OmegaEngine.Graphics.Renderables;

public class TerrainTest : EngineTestBase
{
    private const int Size = 33, StretchH = 10, StretchV = 1;

    /// <summary>
    /// Creates a flat terrain at height <paramref name="height"/>, spanning X 0..320 and Z -320..0.
    /// </summary>
    private Terrain CreateFlatTerrain(byte height = 50)
    {
        var heightMap = new ByteGrid(Size, Size);
        for (int x = 0; x < Size; x++)
            for (int y = 0; y < Size; y++)
                heightMap[x, y] = height;

        return Terrain.Create(Engine, new(Size, Size), StretchH, StretchV,
            heightMap, new NibbleGrid(Size / 3, Size / 3), [null], occlusionIntervalMap: null,
            lighting: false, blockSize: 16);
    }

    [Fact]
    public void IntersectsHitsTerrainFromAbove()
    {
        using var terrain = CreateFlatTerrain(height: 50);

        // Straight down onto the middle of the terrain
        var ray = new Ray(new Vector3(160, 500, -160), -Vector3.UnitY);

        terrain.Intersects(ray, out float distance).Should().BeTrue();
        distance.Should().BeApproximately(500 - 50 * StretchV, 0.5f);
    }

    [Fact]
    public void IntersectsMissesBesideTerrain()
    {
        using var terrain = CreateFlatTerrain();

        // Straight down, but well outside the terrain's horizontal extent
        var ray = new Ray(new Vector3(-500, 500, -160), -Vector3.UnitY);

        terrain.Intersects(ray, out float _).Should().BeFalse();
    }

    [Fact]
    public void IntersectsMissesWhenPointingAway()
    {
        using var terrain = CreateFlatTerrain();

        // Above the terrain, pointing up
        var ray = new Ray(new Vector3(160, 500, -160), Vector3.UnitY);

        terrain.Intersects(ray, out float _).Should().BeFalse();
    }

    [Fact]
    public void IntersectsReturnsNearestOfSeveralHits()
    {
        using var terrain = CreateFlatTerrain(height: 50);

        // A shallow ray crosses many blocks; the nearest hit must win
        var ray = new Ray(new Vector3(10, 200, -160), Vector3.Normalize(new(1, -1, 0)));

        terrain.Intersects(ray, out float distance).Should().BeTrue();

        // Verify the reported distance really lands on the terrain surface
        var hit = ray.Position + distance * ray.Direction;
        hit.Y.Should().BeApproximately(50 * StretchV, 0.5f);
    }

    [Fact]
    public void IntersectsStillWorksAfterModifyHeightInvalidatesBoundingBoxes()
    {
        using var terrain = CreateFlatTerrain(height: 50);

        // Raise a patch under the ray; this also invalidates the per-block bounding boxes
        var raised = new byte[4, 4];
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                raised[x, y] = 100;
        terrain.ModifyHeight(new Point(16, 16), raised);

        var ray = new Ray(new Vector3(16 * StretchH, 500, -16 * StretchH), -Vector3.UnitY);

        terrain.Intersects(ray, out float distance).Should().BeTrue();
        distance.Should().BeApproximately(500 - 100 * StretchV, 0.5f,
            "the RAM copy used for picking should reflect the new height");
    }
}

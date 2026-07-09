/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Drawing;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Graphics.Shaders;

namespace OmegaEngine.Graphics;

// This file contains code for sorting (and caching) bodies based on their distance to the camera
partial class View
{
    #region Cache lists
    // Note: Using Lists here, because the size of the internal arrays will auto-optimize after a few frames

    /// <summary>
    /// A list of enabled (<see cref="PostShader.Enabled"/>) <see cref="PostShader"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="PostShaders"/>.
    /// Cache for a single frame, used in <see cref="Render"/>
    /// </remarks>
    private readonly List<PostShader> _effectivePostShaders = new(capacity: 5);

    /// <summary>
    /// A front-to-back sorted list of visible <see cref="PositionableRenderable"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="OmegaEngine.Graphics.Scene.Positionables"/>.
    /// Cache for a single frame, used in <see cref="RenderScene"/> and <see cref="Pick(Point)"/>
    /// </remarks>
    private readonly List<PositionableRenderable> _sortedBodies = new(capacity: 50);

    /// <summary>
    /// A front-to-back sorted list of visible <see cref="Water"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="_sortedBodies"/>.
    /// Cache for a single frame, used in <see cref="RenderScene"/>
    /// </remarks>
    private readonly List<Water> _sortedWaters = new(capacity: 10);

    /// <summary>
    /// A front-to-back sorted list of visible <see cref="Terrain"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="_sortedBodies"/>.
    /// Cache for a single frame, used in <see cref="RenderScene"/>
    /// </remarks>
    private readonly List<Terrain> _sortedTerrains = new(capacity: 1);

    /// <summary>
    /// A front-to-back sorted list of visible, transparent (<see cref="Renderable.Alpha"/>) <see cref="PositionableRenderable"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="_sortedBodies"/>.
    /// Cache for a single frame, used in <see cref="RenderScene"/>
    /// </remarks>
    private readonly List<PositionableRenderable> _sortedTransparentBodies = new(capacity: 10);

    /// <summary>
    /// A front-to-back sorted list of visible, opaque (<see cref="Renderable.Alpha"/>) <see cref="PositionableRenderable"/>s, except for <see cref="Water"/>s and <see cref="Terrain"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="_sortedBodies"/>.
    /// Cache for a single frame, used in <see cref="RenderScene"/>
    /// </remarks>
    private readonly List<PositionableRenderable> _sortedOpaqueBodies = new(capacity: 30);

    /// <summary>
    /// Scratch buffer holding the squared distance to the camera for each entry in <see cref="_sortedBodies"/>, so it only needs to be calculated once per body per frame.
    /// </summary>
    private readonly List<(double DistanceSquared, int Index, PositionableRenderable Body)> _bodySortKeys = new(capacity: 50);

    /// <summary>
    /// Scratch buffer holding the squared distance to the camera for each entry in <see cref="_sortedTerrains"/>, so it only needs to be calculated once per body per frame.
    /// </summary>
    private readonly List<(double DistanceSquared, int Index, Terrain Body)> _terrainSortKeys = new(capacity: 1);
    #endregion

    /// <summary>
    /// Sort the content of <see cref="OmegaEngine.Graphics.Scene.Positionables"/> into multiple cache lists
    /// </summary>
    /// <seealso cref="_sortedBodies"/>
    /// <seealso cref="_sortedTerrains"/>
    /// <seealso cref="_sortedWaters"/>
    /// <seealso cref="_sortedTransparentBodies"/>
    /// <seealso cref="_sortedOpaqueBodies"/>
    private void SortBodies()
    {
        #region Clear caches
        _sortedBodies.Clear();
        _sortedTerrains.Clear();
        _sortedWaters.Clear();
        _sortedTransparentBodies.Clear();
        _sortedOpaqueBodies.Clear();
        _bodySortKeys.Clear();
        _terrainSortKeys.Clear();
        #endregion

        var cameraPosition = Camera.Position;

        #region Build master list
        foreach (PositionableRenderable body in Scene.Positionables)
        {
            // Filter out bodies that don't belong in this type of view
            if (!IsToRender(body)) continue;

            body.OnPreVisibilityCheck();

            // Apply the floating origin here, so sorting will work.
            // We may need to re-apply it later because ChildViews messed with it.
            body.SetFloatingOrigin(Camera);

            // Filter out invisible bodies
            if (!body.IsVisible(Camera)) continue;

            // Calculate the distance once per body, instead of repeatedly during sorting
            double distanceSquared = (body.Position - cameraPosition).LengthSquared();

            // Separate out Terrain bodies early, because they need to be sorted separately
            if (body is Terrain terrain) _terrainSortKeys.Add((distanceSquared, _terrainSortKeys.Count, terrain));
            else _bodySortKeys.Add((distanceSquared, _bodySortKeys.Count, body));
        }

        // Sort the bodies near-to-far (or the other way round if culling is inverted) with the original index as a stable tie-breaker
        _bodySortKeys.Sort(CompareSortKey);
        _terrainSortKeys.Sort(CompareSortKey);

        foreach (var entry in _bodySortKeys) _sortedBodies.Add(entry.Body);
        foreach (var entry in _terrainSortKeys) _sortedTerrains.Add(entry.Body);
        #endregion

        #region Distribute bodies among specialized lists
        foreach (PositionableRenderable body in _sortedBodies)
        {
            // Separate out Water bodies
            if (body is Water water) _sortedWaters.Add(water);

            // Separate out transparent bodies
            else if (body.Alpha <= EngineState.Opaque)
                _sortedOpaqueBodies.Add(body);
            else _sortedTransparentBodies.Add(body);
        }

        // Terrains need to be part of the normal bodies list, just placed at the end, due to special sorting
        _sortedBodies.AddRange(_sortedTerrains);
        #endregion
    }

    private int CompareSortKey((double DistanceSquared, int Index, PositionableRenderable Body) x, (double DistanceSquared, int Index, PositionableRenderable Body) y)
    {
        int compare = x.DistanceSquared.CompareTo(y.DistanceSquared);
        if (InvertCull) compare = -compare;
        return compare != 0 ? compare : x.Index.CompareTo(y.Index);
    }

    private int CompareSortKey((double DistanceSquared, int Index, Terrain Body) x, (double DistanceSquared, int Index, Terrain Body) y)
    {
        int compare = x.DistanceSquared.CompareTo(y.DistanceSquared);
        if (InvertCull) compare = -compare;
        return compare != 0 ? compare : x.Index.CompareTo(y.Index);
    }
}

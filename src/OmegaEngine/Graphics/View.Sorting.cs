/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
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
    private readonly List<PostShader> _effectivePostShaders = new(5); // Use educated guess for list capacity

    /// <summary>
    /// A front-to-back sorted list of visible <see cref="PositionableRenderable"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="OmegaEngine.Graphics.Scene.Positionables"/>.
    /// Cache for a single frame, used in <see cref="RenderScene"/> and <see cref="Pick"/>
    /// </remarks>
    private readonly List<PositionableRenderable> _sortedBodies = new(50); // Use educated guess for list capacity

    /// <summary>
    /// A front-to-back sorted list of visible <see cref="Water"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="_sortedBodies"/>.
    /// Cache for a single frame, used in <see cref="RenderScene"/>
    /// </remarks>
    private readonly List<Water> _sortedWaters = new(10); // Use educated guess for list capacity

    /// <summary>
    /// A front-to-back sorted list of visible <see cref="Terrain"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="_sortedBodies"/>.
    /// Cache for a single frame, used in <see cref="RenderScene"/>
    /// </remarks>
    private readonly List<Terrain> _sortedTerrains = new(1); // Use educated guess for list capacity

    /// <summary>
    /// A front-to-back sorted list of visible, transparent (<see cref="Renderable.Alpha"/>) <see cref="PositionableRenderable"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="_sortedBodies"/>.
    /// Cache for a single frame, used in <see cref="RenderScene"/>
    /// </remarks>
    private readonly List<PositionableRenderable> _sortedTransparentBodies = new(10); // Use educated guess for list capacity

    /// <summary>
    /// A front-to-back sorted list of visible, opaque (<see cref="Renderable.Alpha"/>) <see cref="PositionableRenderable"/>s, except for <see cref="Water"/>s and <see cref="Terrain"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="_sortedBodies"/>.
    /// Cache for a single frame, used in <see cref="RenderScene"/>
    /// </remarks>
    private readonly List<PositionableRenderable> _sortedOpaqueBodies = new(30); // Use educated guess for list capacity
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
        #endregion

        #region Build master list
        foreach (PositionableRenderable body in Scene.Positionables)
        {
            // Filter out bodies that don't belong in this type of view
            if (!IsToRender(body)) continue;

            // Apply the offset here, so sorting will work.
            // We may need to re-apply it later because ChildViews messed with it.
            ApplyCameraBase(body);

            // Filter out invisible bodies
            if (!body.IsVisible(Camera)) continue;

            // Separate out Terrain bodies early, because they need to be sorted separately
            if (body is Terrain terrain) _sortedTerrains.Add(terrain);
            else _sortedBodies.Add(body);
        }

        // Sort the bodies near-to-far (or the other way round if culling is inverted)
        var comparer = new DistanceComparer(Camera, InvertCull);
        _sortedBodies.Sort(comparer);
        _sortedTerrains.Sort(comparer);
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
}

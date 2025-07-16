/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Graphics.Shaders;

namespace OmegaEngine.Graphics
{
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

        #region Sorting algorithms
        /// <summary>
        /// The difference between the distance of <paramref name="body1"/> and <paramref name="body2"/> to the camera
        /// </summary>
        /// <returns>The inverted difference value usable for sorting</returns>
        private static int CameraDistSort(PositionableRenderable body1, PositionableRenderable body2)
        {
            double distance = body1.CurrentCameraDistance - body2.CurrentCameraDistance;

            if (distance > int.MaxValue - 1) return int.MaxValue;
            if (distance < int.MinValue + 2) return int.MinValue + 1;
            return (int)distance;
        }

        /// <summary>
        /// The difference between the distance of <paramref name="body2"/> and <paramref name="body1"/> to the camera
        /// </summary>
        /// <returns>The difference value usable for sorting</returns>
        private static int CameraDistSortInv(PositionableRenderable body1, PositionableRenderable body2)
        {
            double distance = body2.CurrentCameraDistance - body1.CurrentCameraDistance;

            if (distance > int.MaxValue - 1) return int.MaxValue;
            if (distance < int.MinValue + 2) return int.MinValue + 1;
            return (int)distance;
        }
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
                var terrain = body as Terrain;
                if (terrain != null) _sortedTerrains.Add(terrain);
                else _sortedBodies.Add(body);
            }

            // Sort the bodies near-to-far (or the other way round if culling is inverted)
            _sortedBodies.Sort(InvertCull ? (Comparison<PositionableRenderable>)CameraDistSortInv : CameraDistSort);
            _sortedTerrains.Sort(InvertCull ? (Comparison<Terrain>)CameraDistSortInv : CameraDistSort);
            #endregion

            #region Distribute bodies among specialized lists
            foreach (PositionableRenderable body in _sortedBodies)
            {
                // Separate out Water bodies
                var water = body as Water;
                if (water != null) _sortedWaters.Add(water);

                    // Separate out transparent bodies
                else if (body.Alpha <= EngineState.Opaque)
                    _sortedOpaqueBodies.Add(body);
                else _sortedTransparentBodies.Add(body);
            }

            // Terrains need to be part of the normal bodies list, just placed at the end, due to special sorting
            _sortedTerrains.ForEach(terrain => _sortedBodies.Add(terrain));
            #endregion
        }
    }
}

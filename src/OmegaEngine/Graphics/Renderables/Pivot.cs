/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using OmegaEngine.Graphics.Cameras;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// A <see cref="PositionableRenderable"/> without any geometry of its own, used to group <see cref="PositionableRenderable.Children"/> under a shared transform.
/// </summary>
/// <remarks>Draws nothing, is skipped by <see cref="View"/> sorting and is never picked; only its transform (<see cref="PositionableRenderable.Position"/>, <see cref="PositionableRenderable.Rotation"/>, <see cref="PositionableRenderable.Scale"/> and <see cref="PositionableRenderable.PreTransform"/>) matters.</remarks>
public sealed class Pivot : PositionableRenderable
{
    /// <summary>
    /// Creates a new pivot.
    /// </summary>
    public Pivot()
    {
        Pickable = false;
    }

    /// <inheritdoc/>
    internal override void Render(Camera camera, GetEffectiveLights? getEffectiveLights = null)
    {
        // Nothing to draw; children are rendered on their own
    }
}

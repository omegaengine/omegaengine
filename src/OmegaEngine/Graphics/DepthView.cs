/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using OmegaEngine.Graphics.Renderables;

namespace OmegaEngine.Graphics;

/// <summary>
/// A <see cref="SpecialView"/> for rendering depth maps
/// </summary>
public sealed class DepthView : SpecialView
{
    /// <summary>
    /// Creates a new view for rendering depth maps
    /// </summary>
    /// <param name="baseView">The <see cref="View"/> to base this depth-view on</param>
    internal DepthView(View baseView) : base(baseView, baseView.Camera)
    {
        // Background (no geometry) represents maximum depth
        BackgroundColor = Color.White;
    }

    /// <inheritdoc/>
    protected override void RenderBody(PositionableRenderable body)
    {
        #region Sanity checks
        if (body == null) throw new ArgumentNullException(nameof(body));
        #endregion

        // Backup the current surface effect and replace it by a special one for depth
        var surfaceEffect = body.SurfaceEffect;
        body.SurfaceEffect = SurfaceEffect.Depth;

        base.RenderBody(body);

        // Restore the original surface effect
        body.SurfaceEffect = surfaceEffect;
    }
}

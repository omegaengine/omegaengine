/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;

namespace OmegaEngine.Graphics;

/// <summary>
/// A <see cref="SpecialView"/>  for rendering glow maps
/// </summary>
public sealed class GlowView : SpecialView
{
    /// <summary>
    /// Creates a new view for rendering glow maps
    /// </summary>
    /// <param name="baseView">The <see cref="View"/> to base this glow-view on</param>
    internal GlowView(View baseView) : base(baseView, baseView.Camera)
    {
        BackgroundColor = Color.Black;
    }

    /// <inheritdoc/>
    internal override RenderPass Pass => RenderPass.Glow;
}

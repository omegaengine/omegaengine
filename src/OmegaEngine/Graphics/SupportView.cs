/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;

namespace OmegaEngine.Graphics;

/// <summary>
/// A common base class for all <see cref="View.ChildViews"/> that provide support functionality like water refraction/reflection, glow maps, etc.
/// </summary>
/// <param name="baseView">The <see cref="View"/> to base this support-view on</param>
/// <param name="camera">The <see cref="Camera"/> to look at the <see cref="Scene"/> with</param>
public abstract class SupportView(View baseView, Camera camera)
    : TextureView(baseView.Scene, camera, SupportSize(baseView.Area.Size))
{
    /// <summary>
    /// The <see cref="View"/> this one is based upon
    /// </summary>
    protected readonly View BaseView = baseView;

    /// <summary>
    /// Calculates the size for a <see cref="SupportView"/> based on the base <see cref="View"/>'s size
    /// </summary>
    /// <param name="size"></param>
    /// <returns>.</returns>
    private static Size SupportSize(Size size) => new(size.Width * 2 / 3, size.Height * 2 / 3);

    /// <inheritdoc/>
    protected override bool IsToRender(PositionableRenderable body)
    {
        #region Sanity checks
        if (body == null) throw new ArgumentNullException(nameof(body));
        #endregion

        return body.RenderIn switch
        {
            ViewType.All or ViewType.SupportOnly => true,
            ViewType.GlowOnly => Name?.EndsWith(" Glow", StringComparison.OrdinalIgnoreCase) ?? false,
            ViewType.NormalOnly => false,
            _ => throw new ArgumentException("Invalid ViewType!", nameof(body))
        };
    }

    /// <inheritdoc/>
    internal override void Render()
    {
        // Keep the size in sync with the base view
        Area = new(new(), SupportSize(BaseView.Area.Size));

        base.Render();
    }
}

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
public abstract class SupportView : TextureView
{
    #region Variables
    /// <summary>
    /// The <see cref="View"/> this one is based upon
    /// </summary>
    protected readonly View BaseView;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new support-view
    /// </summary>
    /// <param name="baseView">The <see cref="View"/> to base this support-view on</param>
    /// <param name="camera">The <see cref="Camera"/> to look at the <see cref="Scene"/> with</param>
    protected SupportView(View baseView, Camera camera) :
        base(baseView.Scene, camera, SupportSize(baseView.Area.Size))
    {
        BaseView = baseView;
    }
    #endregion

    //--------------------//

    #region Size
    /// <summary>
    /// Calculates the size for a <see cref="SupportView"/> based on the base <see cref="View"/>'s size
    /// </summary>
    /// <param name="size"></param>
    /// <returns>.</returns>
    private static Size SupportSize(Size size) => new(size.Width * 2 / 3, size.Height * 2 / 3);
    #endregion

    #region Visibility check
    /// <inheritdoc/>
    protected override bool IsToRender(PositionableRenderable body)
    {
        #region Sanity checks
        if (body == null) throw new ArgumentNullException(nameof(body));
        #endregion

        switch (body.RenderIn)
        {
            case ViewType.All:
            case ViewType.SupportOnly:
                return true;
            case ViewType.GlowOnly:
                return Name.EndsWith(" Glow", StringComparison.OrdinalIgnoreCase);
            case ViewType.NormalOnly:
                return false;
            default:
                throw new ArgumentException("Invalid ViewType!", nameof(body));
        }
    }
    #endregion

    //--------------------//

    #region Render
    /// <inheritdoc/>
    internal override void Render()
    {
        // Keep the size in sync with the base view
        Area = new(new(), SupportSize(BaseView.Area.Size));

        base.Render();
    }
    #endregion
}

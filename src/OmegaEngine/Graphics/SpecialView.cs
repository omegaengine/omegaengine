﻿/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Drawing;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Cameras;

namespace OmegaEngine.Graphics;

/// <summary>
/// A common base class for views that use non-standard rendering modes.
/// </summary>
public abstract class SpecialView : SupportView
{
    #region Properties
    /// <summary>
    /// Not applicable to <see cref="SpecialView"/>.
    /// </summary>
    [Browsable(false)]
    public sealed override bool Fog { get => false; set { } }

    /// <summary>
    /// Not applicable to <see cref="SpecialView"/>.
    /// </summary>
    [Browsable(false)]
    public sealed override bool Lighting { get => false; set { } }
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new special-view
    /// </summary>
    /// <param name="baseView">The <see cref="View"/> to base this support-view on</param>
    /// <param name="camera">The <see cref="Camera"/> to look at the <see cref="Scene"/> with</param>
    protected SpecialView(View baseView, Camera camera) :
        base(baseView, camera)
    {}
    #endregion

    //--------------------//

    #region Render background
    /// <inheritdoc/>
    protected override void RenderBackground()
    {
        using (new ProfilerEvent("Clear ZBuffer and BackBuffer"))
            Engine.Device.Clear(ClearFlags.ZBuffer | ClearFlags.Target, Color.Black, 1.0f, 0);
    }
    #endregion

    //--------------------//

    #region Render
    /// <inheritdoc/>
    internal override void Render()
    {
        // Keep the camera in sync with the base view
        Camera = BaseView.Camera;

        base.Render();
    }
    #endregion
}

/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using NanoByte.Common;
using OmegaEngine.Foundation.Geometry;
using SlimDX;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// A camera that imitates the perspective of another <see cref="Camera"/>.
/// </summary>
/// <param name="parentCamera">The parent camera to track</param>
public class CloneCamera(Camera parentCamera) : Camera
{
    /// <summary>
    /// The parent camera to track
    /// </summary>
    [Description("The parent camera to track"), Category("Behavior")]
    public Camera ParentCamera { get; set; } = parentCamera;

    /// <inheritdoc/>
    public override void Navigate(DoubleVector3 translation = default, DoubleVector3 rotation = default)
    {
        // User input should never be routed here
    }

    /// <inheritdoc/>
    protected override void UpdateView()
    {
        PositionBase = ParentCamera.PositionBase;

        ViewCached = GetView();
        CacheSpecialMatrices();
        ViewFrustumDirty = true;

        base.UpdateView();
    }

    /// <summary>
    /// Gets the current view matrix.
    /// </summary>
    protected virtual Matrix GetView() => ParentCamera.View;

    /// <inheritdoc/>
    protected override void CacheSpecialMatrices()
    {
        SimpleViewCached = ViewCached;
        SimpleViewCached.M41 = SimpleViewCached.M42 = SimpleViewCached.M43 = 0;

        base.CacheSpecialMatrices();
    }

    /// <inheritdoc/>
    protected override void UpdateProjection()
    {
        NearClip = ParentCamera.NearClip;
        FarClip = ParentCamera.FarClip;

        base.UpdateProjection();
    }
}

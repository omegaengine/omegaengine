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
    public override void Navigate(DoubleVector3 translation, DoubleVector3 rotation)
    {
        // User input should never be routed here
    }

    /// <summary>
    /// Update cached versions of <see cref="View"/> and related matrices
    /// </summary>
    protected override void UpdateView()
    {
        // Keep in sync with parent camera
        PositionBase = ParentCamera.PositionBase;

        // Note: External update check, clone and conditionally recalc
        bool update = false;
        ParentCamera.View.To(ref ViewCached, ref update);
        if (!update) return;

        CacheSpecialMatrices();
        ViewFrustumDirty = true;
    }

    /// <summary>
    /// Update cached versions of <see cref="Camera.Projection"/> and related matrices
    /// </summary>
    protected override void UpdateProjection()
    {
        NearClip = ParentCamera.NearClip;
        FarClip = ParentCamera.FarClip;

        base.UpdateProjection();
    }
}

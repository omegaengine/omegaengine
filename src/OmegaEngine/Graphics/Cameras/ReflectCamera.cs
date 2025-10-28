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
/// A camera that reflects the perspective of another <see cref="Camera"/> along a plane.
/// </summary>
/// <param name="parentCamera">The parent camera to track</param>
/// <param name="reflectPlane">The plane along which to reflect the world</param>
public class ReflectCamera(Camera parentCamera, DoublePlane reflectPlane) : CloneCamera(parentCamera)
{
    private Matrix _parentView;

    /// <summary>
    /// A plane alongside which to reflect the camera view
    /// </summary>
    [Description("A plane alongside which to reflect the camera view"), Category("Behavior")]
    public DoublePlane ReflectPlane { get; set; } = reflectPlane;

    /// <summary>
    /// Update cached versions of <see cref="View"/> and related matrices
    /// </summary>
    protected override void UpdateView()
    {
        // Note: External update check, clone and conditionally recalc, doesn't call base methods
        ParentCamera.View.To(ref _parentView, delegate
        {
            // Reflect the view matrix
            var reflectMatrix = Matrix.Reflection(ReflectPlane.ApplyOffset(PositionBase));
            SimpleViewCached = ViewCached = reflectMatrix * _parentView;
            SimpleViewCached.M41 = SimpleViewCached.M42 = SimpleViewCached.M43 = 0;

            CacheSpecialMatrices();
            ViewFrustumDirty = true;
        });
    }
}

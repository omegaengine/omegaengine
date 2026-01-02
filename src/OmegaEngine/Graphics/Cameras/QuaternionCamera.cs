/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NanoByte.Common;
using SlimDX;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// A camera that internally uses quaternions for representing rotations.
/// </summary>
public abstract class QuaternionCamera : Camera
{
    private Quaternion _quaternion = Quaternion.Identity;

    /// <summary>
    /// Quaternion representing the rotation of the camera.
    /// </summary>
    protected Quaternion Quaternion { get => _quaternion; set => value.To(ref _quaternion, ref ViewDirty, ref ViewFrustumDirty); }

    /// <summary>
    /// Update cached versions of <see cref="View"/> and related matrices; abstract, to be overwritten in subclass.
    /// </summary>
    protected override void UpdateView()
    {
        AdjustFloatingOrigin();

        SimpleViewCached = Matrix.RotationQuaternion(_quaternion);
        ViewCached = Matrix.Translation(-PositionCached.ApplyOffset(FloatingOriginCached)) * SimpleViewCached;

        CacheSpecialMatrices();

        base.UpdateView();
    }
}

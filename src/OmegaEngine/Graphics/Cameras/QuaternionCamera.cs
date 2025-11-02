/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using NanoByte.Common;
using SlimDX;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// A camera that internally uses quaternions for representing rotations.
/// </summary>
public abstract class QuaternionCamera : Camera
{
    private Quaternion _viewQuat;

    /// <summary>
    /// The current camera view as a quaternion
    /// </summary>
    [Browsable(false)]
    public Quaternion ViewQuat { get => _viewQuat; set => value.To(ref _viewQuat, ref ViewDirty, ref ViewFrustumDirty); }

    /// <summary>
    /// Update cached versions of <see cref="View"/> and related matrices; abstract, to be overwritten in subclass.
    /// </summary>
    protected override void UpdateView()
    {
        AdjustPositionBase();

        SimpleViewCached = Matrix.RotationQuaternion(_viewQuat);
        ViewCached = Matrix.Translation(-PositionCached.ApplyOffset(PositionBaseCached)) * SimpleViewCached;

        CacheSpecialMatrices();
    }
}

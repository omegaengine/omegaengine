/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Numerics;
using OmegaEngine.Foundation.Geometry;

namespace OmegaEngine.Graphics.Cameras;

partial class Camera
{
    /// <summary>Does <see cref="ViewCached"/> need to be recalculated?</summary>
    protected bool ViewDirty = true;

    protected Matrix4x4 ViewCached;

    /// <summary>
    /// A left-handed view matrix for the current camera setting
    /// </summary>
    protected internal Matrix4x4 View
    {
        get
        {
            UpdateView();
            return ViewCached;
        }
    }

    protected Matrix4x4 SimpleViewCached;

    /// <summary>
    /// A left-handed view matrix with absolutely no translation information
    /// </summary>
    protected internal Matrix4x4 SimpleView
    {
        get
        {
            UpdateView();
            return SimpleViewCached;
        }
    }

    private Matrix4x4 _viewInverse;

    /// <summary>
    /// An inverted view matrix for the current camera setting
    /// </summary>
    protected internal Matrix4x4 ViewInverse
    {
        get
        {
            UpdateView();
            return _viewInverse;
        }
    }

    private Matrix4x4 _viewTranspose;

    /// <summary>
    /// A transposed view matrix for the current camera setting
    /// </summary>
    protected internal Matrix4x4 ViewTranspose
    {
        get
        {
            UpdateView();
            return _viewTranspose;
        }
    }

    private Matrix4x4 _viewInverseTranspose;

    /// <summary>
    /// An inverted and transposed view matrix for the current camera setting
    /// </summary>
    protected internal Matrix4x4 ViewInverseTranspose
    {
        get
        {
            UpdateView();
            return _viewInverseTranspose;
        }
    }

    private Matrix4x4 _sphericalBillboard;

    /// <summary>
    /// A rotation matrix for a faked spherical billboard effect
    /// </summary>
    protected internal Matrix4x4 SphericalBillboard
    {
        get
        {
            UpdateView();
            return _sphericalBillboard;
        }
    }

    private Matrix4x4 _cylindricBillboard;

    /// <summary>
    /// A rotation matrix for a faked cylindrical billboard effect
    /// </summary>
    protected internal Matrix4x4 CylindricalBillboard
    {
        get
        {
            UpdateView();
            return _cylindricBillboard;
        }
    }

    private Plane _effectiveClipPlane;

    /// <summary>
    /// The effective clip plane to use for rendering
    /// </summary>
    internal Plane EffectiveClipPlane
    {
        get
        {
            UpdateView();
            UpdateProjection();
            return _effectiveClipPlane;
        }
    }

    /// <summary>
    /// Update cached versions of <see cref="View"/> and related matrices if necessary
    /// </summary>
    protected virtual void UpdateView()
    {
        ViewDirty = false;
    }

    /// <summary>
    /// Calculate cached versions of special matrices (e.g. <see cref="ViewInverse"/> calculated from <see cref="View"/>)
    /// </summary>
    protected virtual void CacheSpecialMatrices()
    {
        _viewInverse = VectorMath.Invert(ViewCached);
        _viewTranspose = Matrix4x4.Transpose(ViewCached);
        _viewInverseTranspose = Matrix4x4.Transpose(_viewInverse);

        var spherical = Quaternion.CreateFromRotationMatrix(_viewInverse);
        _sphericalBillboard = Matrix4x4.CreateFromQuaternion(spherical);

        var cylindric = Quaternion.Normalize(new(0, spherical.Y, 0, spherical.W));
        _cylindricBillboard = Matrix4x4.CreateFromQuaternion(cylindric);

        _effectiveClipPlane = _clipPlane.ApplyOffset(FloatingOriginCached);
    }
}

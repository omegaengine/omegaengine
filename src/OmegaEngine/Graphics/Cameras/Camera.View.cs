/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SlimDX;

namespace OmegaEngine.Graphics.Cameras;

partial class Camera
{
    /// <summary>Does <see cref="ViewCached"/> need to be recalculated?</summary>
    protected bool ViewDirty = true;

    protected Matrix ViewCached;

    /// <summary>
    /// A left-handed view matrix for the current camera setting
    /// </summary>
    protected internal Matrix View
    {
        get
        {
            UpdateView();
            return ViewCached;
        }
    }

    protected Matrix SimpleViewCached;

    /// <summary>
    /// A left-handed view matrix with absolutely no translation information
    /// </summary>
    protected internal Matrix SimpleView
    {
        get
        {
            UpdateView();
            return SimpleViewCached;
        }
    }

    private Matrix _viewInverse;

    /// <summary>
    /// An inverted view matrix for the current camera setting
    /// </summary>
    protected internal Matrix ViewInverse
    {
        get
        {
            UpdateView();
            return _viewInverse;
        }
    }

    private Matrix _viewTranspose;

    /// <summary>
    /// A transposed view matrix for the current camera setting
    /// </summary>
    protected internal Matrix ViewTranspose
    {
        get
        {
            UpdateView();
            return _viewTranspose;
        }
    }

    private Matrix _viewInverseTranspose;

    /// <summary>
    /// An inverted and transposed view matrix for the current camera setting
    /// </summary>
    protected internal Matrix ViewInverseTranspose
    {
        get
        {
            UpdateView();
            return _viewInverseTranspose;
        }
    }

    private Matrix _sphericalBillboard;

    /// <summary>
    /// A rotation matrix for a faked spherical billboard effect
    /// </summary>
    protected internal Matrix SphericalBillboard
    {
        get
        {
            UpdateView();
            return _sphericalBillboard;
        }
    }

    private Matrix _cylindricBillboard;

    /// <summary>
    /// A rotation matrix for a faked cylindrical billboard effect
    /// </summary>
    protected internal Matrix CylindricalBillboard
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
        _viewInverse = Matrix.Invert(ViewCached);
        _viewTranspose = Matrix.Transpose(ViewCached);
        _viewInverseTranspose = Matrix.Transpose(_viewInverse);

        var spherical = Quaternion.RotationMatrix(_viewInverse);
        _sphericalBillboard = Matrix.RotationQuaternion(spherical);

        var cylindric = Quaternion.Normalize(new(0, spherical.Y, 0, spherical.W));
        _cylindricBillboard = Matrix.RotationQuaternion(cylindric);

        _effectiveClipPlane = _clipPlane.ApplyOffset(PositionBaseCached);
    }
}

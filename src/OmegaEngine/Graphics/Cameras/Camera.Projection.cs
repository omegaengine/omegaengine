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
    /// <summary>Does <see cref="_projection"/> need to be recalculated?</summary>
    protected bool ProjectionDirty = true;

    private Matrix _projection;

    /// <summary>
    /// A projection matrix for the current camera setting
    /// </summary>
    internal Matrix Projection
    {
        get
        {
            UpdateProjection();
            return _projection;
        }
    }

    private Matrix _simpleProjection;

    /// <summary>
    /// A projection matrix overriding the clip planes (defaulting to near=1 and far=10)
    /// </summary>
    internal Matrix SimpleProjection
    {
        get
        {
            UpdateProjection();
            return _simpleProjection;
        }
    }

    private Matrix _projectionInverse;

    /// <summary>
    /// An inverted projection matrix for the current camera setting
    /// </summary>
    internal Matrix ProjectionInverse
    {
        get
        {
            UpdateProjection();
            return _projectionInverse;
        }
    }

    private Matrix _projectionTranspose;

    /// <summary>
    /// A transposed projection matrix for the current camera setting
    /// </summary>
    internal Matrix ProjectionTranspose
    {
        get
        {
            UpdateProjection();
            return _projectionTranspose;
        }
    }

    private Matrix _projectionInverseTranspose;

    /// <summary>
    /// An inverted and transposed projection matrix for the current camera setting
    /// </summary>
    internal Matrix ProjectionInverseTranspose
    {
        get
        {
            UpdateProjection();
            return _projectionInverseTranspose;
        }
    }

    /// <summary>
    /// Update <see cref="Projection"/> if necessary
    /// </summary>
    protected virtual void UpdateProjection()
    {
        if (!ProjectionDirty) return;

        _projection = Matrix.PerspectiveFovLH(_fieldOfView, (float)_size.Width / _size.Height, _nearClip, _farClip);
        _simpleProjection = Matrix.PerspectiveFovLH(_fieldOfView, (float)_size.Width / _size.Height, 1, 10);

        // Cache special matrices
        _projectionInverse = Matrix.Invert(_projection);
        _projectionTranspose = Matrix.Transpose(_projection);
        _projectionInverseTranspose = Matrix.Transpose(_projectionInverse);

        _effectiveClipPlane = _clipPlane.ApplyOffset(PositionBaseCached);

        ProjectionDirty = false;
    }
}

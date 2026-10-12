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
    private Matrix4x4 _viewProjection;

    /// <summary>
    /// A view-projection matrix for the current camera setting
    /// </summary>
    internal Matrix4x4 ViewProjection
    {
        get
        {
            UpdateViewProjection();
            return _viewProjection;
        }
    }

    private Matrix4x4 _viewProjectionInverse;

    /// <summary>
    /// An inverted view-projection matrix for the current camera setting
    /// </summary>
    internal Matrix4x4 ViewProjectionInverse
    {
        get
        {
            UpdateViewProjection();
            return _viewProjectionInverse;
        }
    }

    private Matrix4x4 _viewProjectionTranspose;

    /// <summary>
    /// A transposed view-projection matrix for the current camera setting
    /// </summary>
    internal Matrix4x4 ViewProjectionTranspose
    {
        get
        {
            UpdateViewProjection();
            return _viewProjectionTranspose;
        }
    }

    private Matrix4x4 _viewProjectionInverseTranspose;

    /// <summary>
    /// An inverted and transposed view-projection matrix for the current camera setting
    /// </summary>
    internal Matrix4x4 ViewProjectionInverseTranspose
    {
        get
        {
            UpdateViewProjection();
            return _viewProjectionInverseTranspose;
        }
    }

    /// <summary>
    /// Recalculate cached versions of <see cref="_viewProjection"/> and related matrices
    /// </summary>
    private void UpdateViewProjection()
    {
        // Recalculate only when necessary
        (View * Projection).To(ref _viewProjection, delegate
        {
            // Cache special matrices
            _viewProjectionInverse = VectorMath.Invert(_viewProjection);
            _viewProjectionTranspose = Matrix4x4.Transpose(_viewProjection);
            _viewProjectionInverseTranspose = Matrix4x4.Transpose(_viewProjectionInverse);
        });
    }
}

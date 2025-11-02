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

partial class Camera
{
    private Matrix _viewProjection;

    /// <summary>
    /// A view-projection matrix for the current camera setting
    /// </summary>
    internal Matrix ViewProjection
    {
        get
        {
            UpdateViewProjection();
            return _viewProjection;
        }
    }

    private Matrix _viewProjectionInverse;

    /// <summary>
    /// An inverted view-projection matrix for the current camera setting
    /// </summary>
    internal Matrix ViewProjectionInverse
    {
        get
        {
            UpdateViewProjection();
            return _viewProjectionInverse;
        }
    }

    private Matrix _viewProjectionTranspose;

    /// <summary>
    /// A transposed view-projection matrix for the current camera setting
    /// </summary>
    internal Matrix ViewProjectionTranspose
    {
        get
        {
            UpdateViewProjection();
            return _viewProjectionTranspose;
        }
    }

    private Matrix _viewProjectionInverseTranspose;

    /// <summary>
    /// An inverted and transposed view-projection matrix for the current camera setting
    /// </summary>
    internal Matrix ViewProjectionInverseTranspose
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
            _viewProjectionInverse = Matrix.Invert(_viewProjection);
            _viewProjectionTranspose = Matrix.Transpose(_viewProjection);
            _viewProjectionInverseTranspose = Matrix.Transpose(_viewProjectionInverse);
        });
    }
}

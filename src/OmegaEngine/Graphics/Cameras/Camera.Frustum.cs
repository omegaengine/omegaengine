/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using SlimDX;

namespace OmegaEngine.Graphics.Cameras;

partial class Camera
{
    /// <summary>Does the view frustum need to be recalculated?</summary>
    protected bool ViewFrustumDirty = true;

    private readonly Plane[] _viewFrustum = new Plane[6];

    /// <summary>
    /// Update <see cref="_viewFrustum"/> if necessary
    /// </summary>
    private void UpdateViewFrustum()
    {
        // Recalculate only when necessary
        UpdateView();
        if (!ViewFrustumDirty) return;

        // Left plane
        _viewFrustum[0].Normal.X = ViewProjection.M14 + ViewProjection.M11;
        _viewFrustum[0].Normal.Y = ViewProjection.M24 + ViewProjection.M21;
        _viewFrustum[0].Normal.Z = ViewProjection.M34 + ViewProjection.M31;
        _viewFrustum[0].D = ViewProjection.M44 + ViewProjection.M41;

        // Right plane
        _viewFrustum[1].Normal.X = ViewProjection.M14 - ViewProjection.M11;
        _viewFrustum[1].Normal.Y = ViewProjection.M24 - ViewProjection.M21;
        _viewFrustum[1].Normal.Z = ViewProjection.M34 - ViewProjection.M31;
        _viewFrustum[1].D = ViewProjection.M44 - ViewProjection.M41;

        // Top plane
        _viewFrustum[2].Normal.X = ViewProjection.M14 - ViewProjection.M12;
        _viewFrustum[2].Normal.Y = ViewProjection.M24 - ViewProjection.M22;
        _viewFrustum[2].Normal.Z = ViewProjection.M34 - ViewProjection.M32;
        _viewFrustum[2].D = ViewProjection.M44 - ViewProjection.M42;

        // Bottom plane
        _viewFrustum[3].Normal.X = ViewProjection.M14 + ViewProjection.M12;
        _viewFrustum[3].Normal.Y = ViewProjection.M24 + ViewProjection.M22;
        _viewFrustum[3].Normal.Z = ViewProjection.M34 + ViewProjection.M32;
        _viewFrustum[3].D = ViewProjection.M44 + ViewProjection.M42;

        // Near plane
        if (_clipPlane == default)
        {
            _viewFrustum[4].Normal.X = ViewProjection.M13;
            _viewFrustum[4].Normal.Y = ViewProjection.M23;
            _viewFrustum[4].Normal.Z = ViewProjection.M33;
            _viewFrustum[4].D = ViewProjection.M43;
        }
        else _viewFrustum[4] = EffectiveClipPlane;

        // Far plane
        _viewFrustum[5].Normal.X = ViewProjection.M14 - ViewProjection.M13;
        _viewFrustum[5].Normal.Y = ViewProjection.M24 - ViewProjection.M23;
        _viewFrustum[5].Normal.Z = ViewProjection.M34 - ViewProjection.M33;
        _viewFrustum[5].D = ViewProjection.M44 - ViewProjection.M43;

        // Normalize planes
        for (int i = 0; i < 6; i++)
            _viewFrustum[i] = Plane.Normalize(_viewFrustum[i]);

        ViewFrustumDirty = false;
    }

    /// <summary>
    /// Checks whether a <see cref="BoundingSphere"/> is inside the camera's view frustum
    /// </summary>
    /// <param name="boundingSphere">A sphere that completely encompasses the body in world space</param>
    /// <returns><c>true</c> if the sphere is in the frustum</returns>
    internal bool InFrustum(BoundingSphere boundingSphere)
    {
        // Pre-checks
        if (!FrustumCulling) return true;

        UpdateViewFrustum();

        // Check if the sphere lies completely behind one of the frustum planes
        return _viewFrustum.All(t => BoundingSphere.Intersects(boundingSphere, t) != PlaneIntersectionType.Back);

        // Otherwise the object is at least partially visible
    }

    /// <summary>
    /// Checks whether a <see cref="BoundingBox"/> is inside the camera's view frustum
    /// </summary>
    /// <param name="boundingBox">An axis-aligned box that completely encompasses the body in world space</param>
    /// <returns><c>true</c> if the box is in the frustum</returns>
    internal bool InFrustum(BoundingBox boundingBox)
    {
        // Pre-checks
        if (!FrustumCulling) return true;
        if (boundingBox.Diagonal() == default) return true;

        UpdateViewFrustum();

        // Check if the box lies completely behind one of the frustum planes
        return _viewFrustum.All(plane => BoundingBox.Intersects(boundingBox, plane) != PlaneIntersectionType.Back);

        // Otherwise the object is at least partially visible
    }

    /// <summary>
    /// Checks whether a <see cref="BoundingSphere"/> would appear at least 1 pixel in diameter
    /// </summary>
    internal bool AtLeastOnePixelWide(BoundingSphere boundingSphere)
    {
        double distanceToCamera = (boundingSphere.Center - PositionCached.ApplyOffset(FloatingOriginCached)).Length();
        if (distanceToCamera <= boundingSphere.Radius) return true;

        double angularDiameter = 2 * Math.Asin(boundingSphere.Radius / distanceToCamera);
        double pixelDiameter = angularDiameter * _size.Width / _fieldOfView;
        return pixelDiameter >= 1.0;
    }
}

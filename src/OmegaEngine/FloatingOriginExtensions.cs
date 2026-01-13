/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Diagnostics.Contracts;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Cameras;
using SlimDX;

namespace OmegaEngine;

/// <summary>
/// Contains extension methods for <see cref="IFloatingOriginAware"/>.
/// </summary>
internal static class FloatingOriginExtensions
{
    /// <summary>
    /// Returns <see cref="IFloatingOriginAware.FloatingOrigin"/>.
    /// </summary>
    [Pure]
    public static DoubleVector3 GetFloatingOrigin(this IFloatingOriginAware obj)
        => obj.FloatingOrigin;

    /// <summary>
    /// Sets <see cref="IFloatingOriginAware.FloatingOrigin"/>.
    /// </summary>
    public static void SetFloatingOrigin(this IFloatingOriginAware obj, DoubleVector3 origin)
        => obj.FloatingOrigin = origin;

    /// <summary>
    /// Sets <see cref="IFloatingOriginAware.FloatingOrigin"/> to <see cref="Camera.FloatingOrigin"/>.
    /// </summary>
    public static void SetFloatingOrigin(this IFloatingOriginAware obj, Camera camera)
        => obj.FloatingOrigin = camera.FloatingOrigin;

    /// <summary>
    /// Returns <see cref="IFloatingOriginAware.FloatingPosition"/>.
    /// </summary>
    [Pure]
    public static Vector3 GetFloatingPosition(this IFloatingOriginAware obj)
        => obj.FloatingPosition;

    /// <summary>
    /// Applies the <see cref="IFloatingOriginAware.FloatingOrigin"/> to a position, transforming it into a floating position.
    /// </summary>
    [Pure]
    public static Vector3 ApplyFloatingOriginTo(this IFloatingOriginAware obj, DoubleVector3 vector)
        => vector.ApplyOffset(obj.FloatingOrigin);
}

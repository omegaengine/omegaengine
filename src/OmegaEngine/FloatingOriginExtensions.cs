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
using System.Numerics;

namespace OmegaEngine;

/// <summary>
/// Contains extension methods for <see cref="IFloatingOriginAware"/>.
/// </summary>
internal static class FloatingOriginExtensions
{
    extension(IFloatingOriginAware obj)
    {
        /// <summary>
        /// Returns <see cref="IFloatingOriginAware.FloatingOrigin"/>.
        /// </summary>
        [Pure]
        public DoubleVector3 GetFloatingOrigin()
            => obj.FloatingOrigin;

        /// <summary>
        /// Sets <see cref="IFloatingOriginAware.FloatingOrigin"/>.
        /// </summary>
        public void SetFloatingOrigin(DoubleVector3 origin)
            => obj.FloatingOrigin = origin;

        /// <summary>
        /// Sets <see cref="IFloatingOriginAware.FloatingOrigin"/> to <see cref="Camera.FloatingOrigin"/>.
        /// </summary>
        public void SetFloatingOrigin(Camera camera)
            => obj.FloatingOrigin = camera.FloatingOrigin;

        /// <summary>
        /// Returns <see cref="IFloatingOriginAware.FloatingPosition"/>.
        /// </summary>
        [Pure]
        public Vector3 GetFloatingPosition()
            => obj.FloatingPosition;

        /// <summary>
        /// Applies the <see cref="IFloatingOriginAware.FloatingOrigin"/> to a position, transforming it into a floating position.
        /// </summary>
        [Pure]
        public Vector3 ApplyFloatingOriginTo(DoubleVector3 vector)
            => vector.ApplyOffset(obj.FloatingOrigin);
    }
}

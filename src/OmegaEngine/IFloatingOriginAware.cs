/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Cameras;
using SlimDX;

namespace OmegaEngine;

/// <summary>
/// An object with a 64-bit position that can be transformed into a 32-bit floating coordinate system before being sent to graphics hardware.
/// </summary>
/// <seealso cref="Camera.FloatingOrigin"/>
internal interface IFloatingOriginAware : IPositionable
{
    /// <summary>
    /// The origin of the floating coordinate system.
    /// </summary>
    DoubleVector3 FloatingOrigin { get; set; }

    /// <summary>
    /// The <see cref="IPositionable.Position"/> transformed into the floating coordinate system.
    /// </summary>
    Vector3 FloatingPosition { get; }
}

/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using OmegaEngine.Foundation.Geometry;
using SlimDX;

namespace OmegaEngine;

/// <summary>
/// A point of view in the world: a position and orientation to look from.
/// </summary>
public interface IViewpoint
{
    /// <summary>
    /// The position in world space.
    /// </summary>
    DoubleVector3 Position { get; }

    /// <summary>
    /// The forward direction as a unit vector.
    /// </summary>
    Vector3 Forward { get; }

    /// <summary>
    /// The up direction as a unit vector.
    /// </summary>
    Vector3 Up { get; }
}

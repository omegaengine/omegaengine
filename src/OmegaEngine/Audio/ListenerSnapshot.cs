/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using OmegaEngine.Foundation.Geometry;
using SlimDX;

namespace OmegaEngine.Audio;

/// <summary>
/// An immutable per-frame snapshot of an <see cref="IViewpoint"/>, read by the audio thread during mixing.
/// </summary>
/// <param name="Position">The listener's position in world space.</param>
/// <param name="Forward">The direction the listener is facing. A unit vector.</param>
/// <param name="Up">The listener's up direction. A unit vector.</param>
internal sealed record ListenerSnapshot(DoubleVector3 Position, Vector3 Forward, Vector3 Up)
{
    /// <summary>A listener at the world origin facing along +Z with +Y up.</summary>
    public static readonly ListenerSnapshot Default = new(Position: new(), Forward: Vector3.UnitZ, Up: Vector3.UnitY);

    public static ListenerSnapshot FromViewpoint(IViewpoint viewpoint)
        => new(viewpoint.Position, viewpoint.Forward, viewpoint.Up);
}

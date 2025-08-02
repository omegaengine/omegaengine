/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Values;
using SlimDX;

namespace OmegaEngine;

/// <summary>
/// An interface to objects that have an offset that can be subtracted from the <see cref="IPositionable.Position"/> get an effective position to use for rendering.
/// </summary>
/// <remarks>This is mainly used to apply <see cref="Camera.PositionBase"/> before converting from double-precision to single-precision floating point numbers.</remarks>
/// <seealso cref="View.ApplyCameraBase"/>
internal interface IPositionableOffset : IPositionable
{
    /// <summary>
    /// A value to be subtracted from <see cref="IPositionable.Position"/> in order gain <see cref="IPositionableOffset.EffectivePosition"/>
    /// </summary>
    DoubleVector3 Offset { get; set; }

    /// <summary>
    /// The sum of <see cref="IPositionable.Position"/> and <see cref="IPositionableOffset.EffectivePosition"/>
    /// </summary>
    Vector3 EffectivePosition { get; }
}

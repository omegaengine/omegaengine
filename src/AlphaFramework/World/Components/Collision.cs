/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using AlphaFramework.World.Positionables;
using OmegaEngine.Values;

namespace AlphaFramework.World.Components;

/// <summary>
/// Controls how <see cref="EntityBase{TCoordinates,TTemplate}"/>s occupy space around them.
/// </summary>
public abstract class Collision<TCoordinates> : ICloneable
    where TCoordinates : struct
{
    /// <inheritdoc/>
    public override string ToString()
    {
        return GetType().Name;
    }

    /// <summary>
    /// Determines whether a certain point lies within the collision body.
    /// </summary>
    /// <param name="point">The point to check for collision in entity space.</param>
    /// <param name="rotation">How the collision body shall be rotated before performing the collision test.</param>
    /// <returns><c>true</c> if <paramref name="point"/> does collide with the body, <c>false</c> otherwise.</returns>
    public abstract bool CollisionTest(TCoordinates point, float rotation);

    /// <summary>
    /// Determines whether a certain area lies within the collision body.
    /// </summary>
    /// <param name="area">The area to check for collision in entity space.</param>
    /// <param name="rotation">How the collision body shall be rotated before performing the collision test.</param>
    /// <returns><c>true</c> if <paramref name="area"/> does collide with the body, <c>false</c> otherwise.</returns>
    public abstract bool CollisionTest(Quadrangle area, float rotation);

    #region Clone
    /// <summary>
    /// Creates a copy of this <see cref="Collision{TCoordinates}"/>.
    /// </summary>
    /// <returns>The cloned <see cref="Collision{TCoordinates}"/>.</returns>
    public Collision<TCoordinates> Clone()
    {
        // Perform initial shallow copy
        return (Collision<TCoordinates>)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion
}

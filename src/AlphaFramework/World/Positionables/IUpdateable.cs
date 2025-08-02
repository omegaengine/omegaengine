/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace AlphaFramework.World.Positionables;

/// <summary>
/// A <see cref="Positionable{TCoordinates}"/> that updates itself on each frame.
/// </summary>
public interface IUpdateable
{
    /// <summary>
    /// Perform time-dependant state updates.
    /// </summary>
    /// <param name="elapsedTime">How much game time in seconds has elapsed since this method was last called.</param>
    void Update(double elapsedTime);
}

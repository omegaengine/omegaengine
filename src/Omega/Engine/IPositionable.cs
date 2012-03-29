/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Common.Values;

namespace OmegaEngine
{
    /// <summary>
    /// An interface to objects that have a position.
    /// </summary>
    public interface IPositionable
    {
        /// <summary>
        /// The object's position
        /// </summary>
        DoubleVector3 Position { get; set; }
    }
}

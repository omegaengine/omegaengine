/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;

namespace AlphaFramework.World.Positionables
{
    /// <summary>
    /// A marker that designers can leave in the map to remember stuff. Will be ignored in the actual game.
    /// </summary>
    /// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
    public class Memo<TCoordinates> : Positionable<TCoordinates>
        where TCoordinates : struct
    {
        /// <summary>
        /// A short text describing the memo.
        /// </summary>
        [Description("A short text describing the memo.")]
        public string Text { get; set; }
    }
}

/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using LuaInterface;

namespace AlphaFramework.World
{
    /// <summary>
    /// A common base for game sessions (i.e. a game actually being played).
    /// </summary>
    /// <typeparam name="TUniverse">The specific type of <see cref="IUniverse"/> stored in the session.</typeparam>
    public abstract class SessionBase<TUniverse>
        where TUniverse : class, IUniverse
    {
        /// <summary>
        /// The current state of the game world.
        /// </summary>
        [LuaHide]
        public TUniverse Universe { get; set; }

        /// <summary>
        /// The filename of the map file the <see cref="Universe"/> was loaded from.
        /// </summary>
        public string MapSourceFile { get; set; }

        /// <summary>
        ///  Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        protected SessionBase()
        {}

        /// <summary>
        /// Creates a new game session based upon a given universe
        /// </summary>
        /// <param name="baseUniverse">The universe to base the new game session on.</param>
        protected SessionBase(TUniverse baseUniverse)
        {
            #region Sanity checks
            if (baseUniverse == null) throw new ArgumentNullException("baseUniverse");
            #endregion

            Universe = baseUniverse;

            // Transfer map name from universe to session, because it will persist there
            MapSourceFile = baseUniverse.SourceFile;
        }
    }
}

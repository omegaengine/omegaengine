/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using AlphaFramework.World.Positionables;
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
        /// Total elapsed real time in seconds.
        /// </summary>
        public double RealTime { get; set; }

        /// <summary>
        /// Total elapsed game time in seconds.
        /// </summary>
        public double GameTime { get; set; }

        /// <summary>
        /// The factor by which <see cref="GameTime"/> (not <see cref="RealTime"/>) progression should be multiplied.
        /// </summary>
        /// <remarks>This multiplication is not done in <see cref="Update"/>!</remarks>
        [DefaultValue(1f)]
        public float TimeWarpFactor { get; set; }

        /// <summary>
        ///  Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        protected SessionBase()
        {
            TimeWarpFactor = 1;
        }

        /// <summary>
        /// Creates a new game session based upon a given universe
        /// </summary>
        /// <param name="baseUniverse">The universe to base the new game session on.</param>
        protected SessionBase(TUniverse baseUniverse)
        {
            #region Sanity checks
            if (baseUniverse == null) throw new ArgumentNullException("baseUniverse");
            #endregion

            TimeWarpFactor = 1;
            Universe = baseUniverse;

            // Transfer map name from universe to session, because it will persist there
            MapSourceFile = baseUniverse.SourceFile;
        }

        /// <summary>
        /// Updates the session, the contained <see cref="Universe"/> and all <see cref="Positionable{TCoordinates}"/>s in it.
        /// </summary>
        /// <param name="elapsedRealTime">How much real time in seconds has elapsed since this method was last called.</param>
        /// <param name="elapsedGameTime">How much game time in seconds has elapsed since this method was last called.</param>
        /// <remarks>This needs to be called as a part of the render loop.</remarks>
        public virtual void Update(double elapsedRealTime, double elapsedGameTime)
        {
            RealTime += elapsedRealTime;
            GameTime += elapsedGameTime;

            Universe.Update(elapsedGameTime);
        }
    }
}

/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using LuaInterface;
using TemplateWorld.Positionables;

namespace TemplateWorld
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

/*
 * Copyright 2006-2014 Bastian Eicher
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
using AlphaFramework.World;
using Common.Utils;

namespace FrameOfReference.World
{
    /// <summary>
    /// Represents a game session (i.e. a game actually being played).
    /// It is equivalent to the content of a savegame.
    /// </summary>
    public sealed partial class Session : SessionBase<Universe>
    {
        /// <summary>
        /// Creates a new game session based upon a given <see cref="Universe"/>.
        /// </summary>
        /// <param name="baseUniverse">The universe to base the new game session on.</param>
        public Session(Universe baseUniverse) : base(baseUniverse)
        {}

        #region Update
        /// <summary>Fixed step size for updates in seconds. Makes updates deterministic.</summary>
        private const double UpdateStepSize = 0.015;

        /// <summary>The maximum number of seconds to handle in one call to <see cref="Update"/>. Additional time is simply dropped.</summary>
        private const double MaximumUpdate = 0.75;

        /// <summary>
        /// <see cref="UniverseBase{T}.GameTime"/> time left over from the last <see cref="Update"/> call due to the fixed update step size.
        /// </summary>
        [DefaultValue(0.0)]
        public double LeftoverGameTime { get; set; }

        /// <inheritdoc/>
        public override double Update(double elapsedRealTime)
        {
            double elapsedGameTime = (elapsedRealTime * TimeWarpFactor);

            LeftoverGameTime += elapsedGameTime.Clamp(-MaximumUpdate, MaximumUpdate);
            while (Math.Abs(LeftoverGameTime) >= UpdateStepSize)
            {
                // Handle negative time
                double effectiveStep = Math.Sign(LeftoverGameTime) * UpdateStepSize;

                Universe.Update(effectiveStep);
                LeftoverGameTime -= effectiveStep;
            }

            return elapsedGameTime;
        }
        #endregion
    }
}
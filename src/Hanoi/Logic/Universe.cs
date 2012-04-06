/*
 * Copyright 2006-2012 Bastian Eicher
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
using System.Collections.Generic;

namespace Hanoi.Logic
{
    /// <summary>
    /// Keeps track of the simulation universe
    /// </summary>
    public class Universe : ICloneable
    {
        #region Variables
        private readonly Peg[] _pegs;
        private readonly int _numberDiscs;

        public Universe Ancestor;
        #endregion

        #region Properties
        /// <summary>
        /// Total number of discs in the simulation
        /// </summary>
        public int NumberPegs { get { return _pegs.Length; } }

        /// <summary>
        /// Total number of discs in the simulation
        /// </summary>
        public int NumberDiscs { get { return _numberDiscs; } }

        /// <summary>
        /// Are the discs in a default startup position?
        /// </summary>
        public bool OrderedSet { get; private set; }

        /// <summary>
        /// The last disc that was moved
        /// </summary>
        public Disc LastMovedDisc { get; private set; }

        /// <summary>
        /// The last peg a disc was moved from
        /// </summary>
        public Peg LastSourcePeg { get; private set; }

        /// <summary>
        /// How much time has elapsed since the last step was performed
        /// </summary>
        public double StepTime { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a game of "Towers of Hanoi" arbitrarily prefilled with pegs and discs on them
        /// </summary>
        /// <param name="prefinedPegs">The number of discs</param>
        public Universe(IEnumerable<Peg> prefinedPegs)
        {
            // Copy all pegs to an intermediate cache
            var pegCache = new List<Peg>();
            foreach (Peg peg in prefinedPegs)
                pegCache.Add(peg);

            // Transfer pegs from cache to final array
            _pegs = new Peg[pegCache.Count];
            pegCache.CopyTo(_pegs);

            // Sum up all discs
            foreach (Peg peg in _pegs)
                _numberDiscs += peg.DiscCount;

            // Sanity checks
            if (pegCache.Count < 3) throw new ArgumentException("The minimum number of pegs is 3.", "prefinedPegs");
            if (_numberDiscs < 1) throw new ArgumentException("The minimum number of discs is 1.", "prefinedPegs");
        }

        /// <summary>
        /// Creates a default game of "Towers of Hanoi" with a arbitrary number of pegs/towers
        /// </summary>
        /// <param name="numberPegs">The number of pegs/towers</param>
        /// <param name="numberDiscs">The number of discs</param>
        public Universe(byte numberPegs, byte numberDiscs)
        {
            // Sanity checks
            if (numberPegs < 3) throw new ArgumentException("The minimum number of pegs is 3.", "numberPegs");
            if (numberDiscs < 1) throw new ArgumentException("The minimum number of discs is 1.", "numberDiscs");

            // Create the pegs
            _pegs = new Peg[numberPegs];
            for (int i = 0; i < numberPegs; i++)
                _pegs[i] = new Peg();

            // Fill first peg with discs
            _numberDiscs = numberDiscs;
            for (int size = numberDiscs - 1; size >= 0; size--)
                _pegs[0].AddDisc(new Disc((byte)size));

            // Mark this universe as ordered (as opposed to arbitrarily filled)
            OrderedSet = true;
        }

        /// <summary>
        /// Creates a default game of "Towers of Hanoi"
        /// </summary>
        /// <param name="discs">The number of discs</param>
        public Universe(byte discs) : this(3, discs)
        {}
        #endregion

        //--------------------//

        #region Update
        public void Update(double elapsedGameTime)
        {
            StepTime += elapsedGameTime;
        }
        #endregion

        #region Access
        /// <summary>
        /// Gets an array of pegs in this universe
        /// </summary>
        public Peg[] GetPegs()
        {
            var pegArray = new Peg[_pegs.Length];
            _pegs.CopyTo(pegArray, 0);
            return _pegs;
        }

        /// <summary>
        /// Determines the index of a peg in this universe
        /// </summary>
        /// <param name="peg">The peg to find</param>
        public int GetPegIndex(Peg peg)
        {
            for (int i = 0; i < _pegs.Length; i++)
                if (peg == _pegs[i]) return i;
            throw new ArgumentException("Peg not found in Universe", "peg");
        }
        #endregion

        #region Control
        /// <summary>
        /// Moves the top-most disc from one peg to another
        /// </summary>
        /// <param name="source">The peg to take the disc from</param>
        /// <param name="target">The peg to place the disc on</param>
        /// <exception cref="ArgumentException">Thrown if the disc won't fit</exception>
        public void MoveDisc(Peg source, Peg target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");

            Disc disc = source.RemoveTopDisc();
            try
            {
                target.AddDisc(disc);
            }
            catch (ArgumentException)
            {
                source.AddDisc(disc);
                throw;
            }

            LastSourcePeg = source;
            LastMovedDisc = disc;
            StepTime = 0;
            OrderedSet = false;
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this simulation universe
        /// </summary>
        /// <returns>The new simulation universe</returns>
        public Universe Clone()
        {
            // Clone all pegs
            var clonedPegs = new Peg[_pegs.Length];
            for (int i = 0; i < _pegs.Length; i++)
                clonedPegs[i] = _pegs[i].Clone();

            // Create new simulation universe filled with the cloned pegs
            return new Universe(clonedPegs) {OrderedSet = OrderedSet, Ancestor = Ancestor};
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}

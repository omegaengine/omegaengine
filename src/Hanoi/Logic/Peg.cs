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
    /// Represents a peg on which discs can be slided
    /// </summary>
    public class Peg : ICloneable
    {
        #region Variables
        private readonly List<Disc> _discs = new List<Disc>();
        #endregion

        #region Properties
        /// <summary>
        /// The number of discs on currently this peg
        /// </summary>
        public int DiscCount { get { return _discs.Count; } }

        /// <summary>
        /// The currently top-most disc
        /// </summary>
        public Disc TopDisc { get { return _discs.Count == 0 ? null : _discs[_discs.Count - 1]; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new empty peg
        /// </summary>
        public Peg()
        {}

        /// <summary>
        /// Creates a new pre-filled peg
        /// </summary>
        /// <param name="discs">The discs to fill the peg with - must be sorted from bottom to top</param>
        public Peg(IEnumerable<Disc> discs)
        {
            if (discs == null) throw new ArgumentNullException("discs");

            foreach (Disc disc in discs)
                AddDisc(disc);
        }
        #endregion

        //--------------------//

        #region Access
        /// <summary>
        /// Gets an array of all discs on this peg sorted from bottom to top
        /// </summary>
        public Disc[] GetDiscs()
        {
            var discArray = new Disc[_discs.Count];
            _discs.CopyTo(discArray, 0);
            return discArray;
        }

        /// <summary>
        /// Checks whether a disc could be added to this peg
        /// </summary>
        /// <param name="disc">The disc to check</param>
        /// <returns>True if the disc could be added</returns>
        public bool TestDisc(Disc disc)
        {
            if (disc == null) throw new ArgumentNullException("disc");

            if (TopDisc == null) return true;
            return TopDisc.Size > disc.Size;
        }

        /// <summary>
        /// Adds a new disc to this peg
        /// </summary>
        /// <param name="disc">The disc to add</param>
        /// <exception cref="ArgumentException">Thrown if the disc won't fit</exception>
        public void AddDisc(Disc disc)
        {
            if (TestDisc(disc))
                _discs.Add(disc);
            else
                throw new ArgumentException("The last disc on the peg is smaller than the disc to be added.", "disc");
        }

        /// <summary>
        /// Removes the top disc from this peg
        /// </summary>
        /// <returns>The disc that was removed</returns>
        public Disc RemoveTopDisc()
        {
            Disc topDisc = TopDisc;
            _discs.Remove(topDisc);
            return topDisc;
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this peg
        /// </summary>
        /// <returns>The new peg</returns>
        public Peg Clone()
        {
            // Clone all discs
            var clonedDiscs = new Disc[_discs.Count];
            for (int i = 0; i < _discs.Count; i++)
                clonedDiscs[i] = _discs[i].Clone();

            // Create new peg filled with the cloned discs
            return new Peg(clonedDiscs);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}

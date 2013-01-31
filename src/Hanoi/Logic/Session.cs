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
using System.Threading;

namespace Hanoi.Logic
{
    /// <summary>
    /// Represents a game session (i.e. a game actually being played)
    /// </summary>
    public class Session
    {
        #region Variables
        private Universe _currentUniverse;
        private Thread _solveThread;
        private double _realTime, _gameTime;
        private float _timeWarpFactor = 1;
        #endregion

        #region Properties
        /// <summary>
        /// The simulation universe as it is at the moment 
        /// </summary>
        public Universe CurrentUniverse { get { return _currentUniverse; } set { _currentUniverse = value; } }

        /// <summary>
        /// Is this session currently being solved?
        /// </summary>
        public bool Solving { get { return (_solveThread != null && _solveThread.IsAlive); } }

        /// <summary>
        /// Total elapsed real time in seconds
        /// </summary>
        public double RealTime { get { return _realTime; } set { _realTime = value; } }

        /// <summary>
        /// Total elapsed game time in seconds
        /// </summary>
        public double GameTime { get { return _gameTime; } set { _gameTime = value; } }

        /// <summary>
        /// The factor by which the game time (not the real time) is multiplied
        /// </summary>
        public float TimeWarpFactor { get { return _timeWarpFactor; } set { _timeWarpFactor = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a blank new game session
        /// </summary>
        public Session()
        {}

        /// <summary>
        /// Creates a new game session based upon a given universe
        /// </summary>
        /// <param name="baseUniverse">The universe to base the new game session on</param>
        public Session(Universe baseUniverse)
        {
            if (baseUniverse == null) throw new ArgumentNullException("baseUniverse");
            _currentUniverse = baseUniverse;
        }
        #endregion

        //--------------------//

        #region Update
        /// <summary>
        /// Updates the game session
        /// </summary>
        /// <param name="elapsedRealTime">Elapsed real time in seconds since last update</param>
        /// <param name="elapsedGameTime">Elapsed game time in seconds since last update</param>
        public void Update(double elapsedRealTime, double elapsedGameTime)
        {
            _realTime += elapsedRealTime;
            _gameTime += elapsedGameTime;

            _currentUniverse.Update(elapsedGameTime);
        }
        #endregion

        //--------------------//

        #region Algorithms

        #region Selection
        private void Solve()
        {
            Peg[] pegs = _currentUniverse.GetPegs();
            if (_currentUniverse.OrderedSet && _currentUniverse.NumberPegs == 3)
            {
                // Apply the recursive algorithm for standard situations
                Recursive(pegs[0].DiscCount, pegs[0], pegs[1], pegs[2]);
            }
            else
            {
                // Apply the brute force agorithms in all other cases
                //Universe solution = BruteForce(currentUniverse, pegs[pegs.Length - 1]);

                // ToDo: Playback
                //if (Solving) Thread.Sleep((int)(1000 / TimeWarpFactor));
                //currentUniverse = solution;
            }
        }
        #endregion

        #region Recursive
        /// <summary>
        /// Recursive algorithm for moving discs from one peg to another using an intermediate peg
        /// </summary>
        /// <param name="numberDiscs">The number of discs to move</param>
        /// <param name="source">The source peg</param>
        /// <param name="intermediate">The intermediate peg</param>
        /// <param name="target">The target peg</param>
        private void Recursive(int numberDiscs, Peg source, Peg intermediate, Peg target)
        {
            if (numberDiscs > 0)
            {
                Recursive(numberDiscs - 1, source, target, intermediate);
                MoveDisc(source, target);
                Recursive(numberDiscs - 1, intermediate, source, target);
            }
        }
        #endregion

        #region Brute force
        /// <summary>
        /// Brute force algorithm for moving discs from all pegs to one single peg using an arbitrary number of intermediate pegs
        /// </summary>
        /// <param name="universe">The universe to test via brute force</param>
        /// <param name="target">The target peg</param>
        /// <returns>The final universe</returns>
        private static Universe BruteForce(Universe universe, Peg target)
        {
            Peg[] pegs = universe.GetPegs();

            foreach (Peg from in pegs)
            {
                foreach (Peg to in pegs)
                {
                    try
                    {
                        Universe clone = universe.Clone();
                        clone.MoveDisc(from, to);
                        clone.Ancestor = universe;
                        Universe solution = BruteForce(clone, target);
                        if (solution != null) return solution;
                        if (target.DiscCount == universe.NumberDiscs)
                            return clone;
                    }
                    catch (ArgumentException)
                    {}
                }
            }

            return null;
        }
        #endregion

        #endregion

        #region Access
        /// <summary>
        /// Moves the top-most disc from one peg to another
        /// </summary>
        /// <param name="source">The peg to take the disc from</param>
        /// <param name="target">The peg to place the disc on</param>
        public void MoveDisc(Peg source, Peg target)
        {
            lock (_currentUniverse)
            {
                _currentUniverse.MoveDisc(source, target);
            }

            if (Solving) Thread.Sleep((int)(1000 / TimeWarpFactor));
        }

        /// <summary>
        /// Solves the puzzle using the optimal algorithm in a seperate thread
        /// </summary>
        public void StartSolving()
        {
            if (Solving)
                throw new InvalidOperationException("Already solving");

            _solveThread = new Thread(Solve);
            _solveThread.Start();
        }

        /// <summary>
        /// Stops the solving thread if it is running
        /// </summary>
        public void StopSolving()
        {
            if (Solving)
            {
                _solveThread.Abort();
                _solveThread = null;
            }
        }
        #endregion
    }
}

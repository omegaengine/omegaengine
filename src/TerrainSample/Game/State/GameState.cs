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

using OmegaEngine;
using TerrainSample.Presentation;
using TerrainSample.World;

namespace TerrainSample.State
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class GameState
    {
        #region Variables
        /// <summary>
        /// The engine to use for rendering.
        /// </summary>
        protected readonly Engine Engine;
        #endregion

        #region Properties
        /// <summary>
        /// Handles the visual representation of <see cref="Session"/> content in the <see cref="OmegaEngine"/>.
        /// </summary>
        public abstract Presenter Presenter { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new game state.
        /// </summary>
        /// <param name="engine">The engine to use for rendering.</param>
        protected GameState(Engine engine)
        {
            Engine = engine;
        }
        #endregion

        //--------------------//

        #region Render
        /// <summary>
        /// Called when the next frame needs to be rendered.
        /// </summary>
        /// <param name="elapsedTime">The number of seconds that have passed since this method was last called.</param>
        public virtual void Render(double elapsedTime)
        {
            // Time passes normally but there is no session
            Engine.Render();
        }
        #endregion
    }
}

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
using OmegaEngine;
using Presentation;
using World;

namespace Core.State
{
    /// <summary>
    /// Represents a state where the game is running an automatic benchmark.
    /// </summary>
    public sealed class BenchmarkState : InGameState
    {
        #region Properties
        private readonly BenchmarkPresenter _presenter;

        /// <inheritdoc/>
        public override Presenter Presenter { get { return _presenter; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new game state.
        /// </summary>
        /// <param name="engine">The engine to use for rendering.</param>
        /// <param name="session">The session to display.</param>
        /// <param name="callback">A delegate to execute after the benchmark is complete with the path of the result file.</param>
        public BenchmarkState(Engine engine, Session session, Action<string> callback) : base(engine, session)
        {
            _presenter = new BenchmarkPresenter(engine, session.Universe, callback);
        }
        #endregion
    }
}

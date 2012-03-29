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

using Common.Storage;
using OmegaEngine;
using Presentation;
using World;

namespace Core.State
{
    /// <summary>
    /// Represents a state where the user is currently playing the game.
    /// </summary>
    public class InGameState : GameState
    {
        #region Properties
        private readonly InGamePresenter _presenter;

        /// <inheritdoc/>
        public override Presenter Presenter { get { return _presenter; } }

        /// <summary>
        /// The game session the user is currently playing.
        /// </summary>
        public Session Session { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new game state.
        /// </summary>
        /// <param name="engine">The engine to use for rendering.</param>
        /// <param name="session">The game session the user is currently playing.</param>       
        public InGameState(Engine engine, Session session) : base(engine)
        {
            Session = session;
            _presenter = new InGamePresenter(engine, session.Universe);
        }
        #endregion

        //--------------------//

        #region Render
        /// <inheritdoc/>
        public override void Render(double elapsedTime)
        {
            // Time passes as defined by the session
            double elapsedGameTime = elapsedTime * Session.TimeWarpFactor;
            Session.Update(elapsedTime, elapsedGameTime);
            Engine.Render(elapsedGameTime);
        }
        #endregion

        #region Save/Load Savegames
        /// <summary>
        /// Saves the <see cref="Session"/> as a savegame stored in the user's profile.
        /// </summary>
        /// <param name="name">The name of the savegame to write.</param>
        public void SaveSession(string name)
        {
            _presenter.PrepareSave();

            string path = Locations.GetSaveDataPath(GeneralSettings.AppName, true, name + Session.FileExt);
            Session.Save(path);
        }

        /// <summary>
        /// Loads a savegame from user's profile to create a new <see cref="InGameState"/>.
        /// </summary>
        /// <param name="engine">The engine to use for rendering.</param>
        /// <param name="name">The name of the savegame to load.</param>
        public static InGameState LoadSession(Engine engine, string name)
        {
            string path = Locations.GetSaveDataPath(GeneralSettings.AppName, true, name + Session.FileExt);
            return new InGameState(engine, Session.Load(path));
        }
        #endregion
    }
}

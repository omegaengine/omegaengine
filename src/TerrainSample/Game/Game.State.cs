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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Common;
using Core.Properties;
using LuaInterface;
using OmegaEngine;
using Presentation;
using World;

namespace Core
{

    #region Game state
    /// <seealso cref="Game.CurrentState"/>
    public enum GameState
    {
        /// <summary>The game is starting up</summary>
        Init,

        /// <summary>The game is in the main menu</summary>
        Menu,

        /// <summary>The game is paused</summary>
        Pause,

        /// <summary>The game is running an automatic benchmark</summary>
        Benchmark,

        /// <summary>The game is normal playing mode</summary>
        InGame,

        /// <summary>The game is in a special live editing mode</summary>
        Modify
    }
    #endregion

    partial class Game
    {
        #region Properties
        /// <summary>
        /// The current state of the game
        /// </summary>
        [LuaHide]
        public GameState CurrentState { get; private set; }
        #endregion

        //--------------------//

        #region Switch to Menu
        /// <summary>
        /// Switches to the main menu
        /// </summary>
        /// <remarks>If <see cref="CurrentState"/> is already <see cref="GameState.Menu"/>, nothing will happen.
        /// Loading will take a while on first call, subsequent calls will be very fast, because <see cref="_menuUniverse"/> is preserved</remarks>
        public void SwitchToMenu()
        {
            // Handle cases where the main menu was bypassed on startup
            if (_menuUniverse == null)
            {
                LoadMenu(GetRandomMenuMap());
                return;
            }

            // Prevent unnecessary loading
            if (CurrentState == GameState.Menu) return;

            CleanupPresenter();
            InitializeMenuMode();
        }
        #endregion

        #region Switch to Game
        /// <summary>
        /// Switches the game to in-game mode
        /// </summary>
        /// <remarks>If <see cref="CurrentState"/> is already <see cref="GameState.InGame"/>, nothing will happen.
        /// Loading may take a while, subsequent calls will be a bit faster because the <see cref="Engine"/> cache will still be hot</remarks>
        public void SwitchToGame()
        {
            #region Sanity checks
            if (CurrentSession == null) throw new InvalidOperationException(Resources.NoSessionLoaded);
            #endregion

            // Prevent unnecessary loading
            if (CurrentState == GameState.InGame) return;

            CleanupPresenter();
            InitializeGameMode();
        }
        #endregion

        #region Switch to Modify
        /// <summary>
        /// Switches the game to map modification mode
        /// </summary>
        /// <remarks>If <see cref="CurrentState"/> is already <see cref="GameState.Modify"/>, nothing will happen.
        /// Loading may take a while, subsequent calls will be a bit faster because the <see cref="Engine"/> cache will still be hot</remarks>
        public void SwitchToModify()
        {
            #region Sanity checks
            if (CurrentSession == null) throw new InvalidOperationException(Resources.NoSessionLoaded);
            #endregion

            // Prevent unnecessary loading
            if (CurrentState == GameState.Modify) return;

            CleanupPresenter();
            InitializeModifyMode();
        }
        #endregion

        #region Toggle Pause
        private GameState _beforePause;

        /// <summary>
        /// Toggles <see cref="CurrentState"/> between <see cref="GameState.InGame"/> and <see cref="GameState.Pause"/>
        /// </summary>
        /// <remarks>When called while <see cref="CurrentState"/> is neither <see cref="GameState.InGame"/> nor <see cref="GameState.Pause"/> nothing happens</remarks>
        public void TogglePause()
        {
            switch (CurrentState)
            {
                case GameState.InGame:
                case GameState.Modify:
                    // Backup previous state
                    _beforePause = CurrentState;

                    CurrentState = GameState.Pause;

                    // Freeze the mouse interaction
                    var interactivePresenter = CurrentPresenter as InteractivePresenter;
                    if (interactivePresenter != null) RemoveInputReceiver(interactivePresenter);

                    // Dim down the screen
                    CurrentPresenter.DimDown();

                    // Show pause menu
                    GuiManager.Reset();
                    LoadDialog("PauseMenu");
                    break;

                case GameState.Pause:
                    // Restore previous state (usually GameState.InGame)
                    CurrentState = _beforePause;

                    // Dim screen back up
                    CurrentPresenter.DimUp();

                    // Restore the mouse interaction
                    AddInputReceiver((InteractivePresenter)CurrentPresenter);

                    // Restore the correct HUD
                    GuiManager.Reset();
                    if (CurrentState == GameState.InGame) LoadDialog("InGame/HUD");
                    else if (CurrentState == GameState.Modify) LoadDialog("InGame/HUD_Modify");
                    break;
            }
        }
        #endregion

        #region Start Benchmark
        /// <summary>
        /// Loads the benchmark map into <see cref="CurrentSession"/> and switches the <see cref="CurrentState"/> to <see cref="GameState.Benchmark"/>
        /// </summary>
        /// <remarks>If <see cref="CurrentState"/> is <see cref="GameState.Benchmark"/>, nothing will happen</remarks>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect", Justification = "This method is only called after level-loads, when a lot of long-lived objects have turned into garbage")]
        private void StartBenchmark()
        {
            // Prevent unnecessary loading
            if (CurrentState == GameState.Benchmark) return;

            using (new TimedLogEvent("Start benchmark"))
            {
                // Load map
                CurrentSession = new Session(Universe.FromContent("Benchmark" + Universe.FileExt));

                // Switch mode
                CurrentState = GameState.Benchmark;

                // Normalize settings for comparable results (don't save to preserve original settings)
                Settings.AutoSave = false;
                Settings.Current.Sound.PlayMusic = false;
                Settings.Current.Graphics.Fading = false;
                Settings.Current.Graphics.WaterEffects = WaterEffectsType.None;
                Settings.Current.Graphics.ParticleSystemQuality = Quality.Low;
                Settings.Current.Display.VSync = false;
                Settings.Current.Display.Resolution = Settings.Current.Display.WindowSize = new Size(800, 600);
#if !DEBUG
                Settings.Current.Display.Fullscreen = true;
#endif

                // Clean up any old stuff
                if (CurrentPresenter != null)
                {
                    var interactivePresenter = CurrentPresenter as InteractivePresenter;
                    if (interactivePresenter != null) RemoveInputReceiver(interactivePresenter);

                    CurrentPresenter.HookOut();
                    if (CurrentPresenter != _menuPresenter) CurrentPresenter.Dispose();
                }

                // Load benchmark universe
                CurrentPresenter = new BenchmarkPresenter(Engine,
                    Universe.FromContent("Benchmark" + Universe.FileExt), path =>
                    { // Callback for sumbitting the benchmark results
                        Form.Visible = false;
                        //if (Msg.Ask(Form, Resources.BenchmarkReady, MsgSeverity.Info, Resources.BenchmarkReadyContinue, Resources.BenchmarkReadyCancel))
                        //{
                        //    // ToDo: File upload
                        //}
                        Msg.Inform(null, string.Format("Please upload the file '{0}'.", path), MsgSeverity.Info);
                        Exit();
                    });
                CurrentPresenter.Initialize();

                // Activate new view
                CurrentPresenter.HookIn();
                if (Settings.Current.Graphics.Fading) Engine.FadeIn();

                // Show benchmark GUI
                GuiManager.Reset();
                LoadDialog("InGame/HUD_Benchmark");
            }

            using (new TimedLogEvent("Clean caches"))
            {
                Engine.Cache.Clean();
                GC.Collect();
            }
        }
        #endregion

        //--------------------//

        #region Initialize Menu
        /// <summary>
        /// Creates the <see cref="_menuPresenter"/> if necessary, sets it as the <see cref="CurrentPresenter"/> and configures the GUI for the main menu
        /// </summary>
        private void InitializeMenuMode()
        {
            Loading = true;

            using (new TimedLogEvent("Initialize menu"))
            {
                // Switch mode
                CurrentState = GameState.Menu;

                // Clean previous presenter
                //CleanupPresenter();

                // Load menu scene
                if (_menuPresenter == null) _menuPresenter = new MenuPresenter(Engine, _menuUniverse);
                _menuPresenter.Initialize();
                CurrentPresenter = _menuPresenter;

                // Activate new view
                CurrentPresenter.HookIn();
                if (Settings.Current.Graphics.Fading) Engine.FadeIn();

                // Show game GUI
                GuiManager.CloseAll();
                LoadDialog("MainMenu");
            }

            Loading = false;
        }
        #endregion

        #region Initialize Game
        /// <summary>
        /// Creates the <see cref="CurrentPresenter"/> and configures the GUI for in-game mode
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect", Justification = "This method is only called after level-loads, when a lot of long-lived objects have turned into garbage")]
        private void InitializeGameMode()
        {
            Loading = true;

            using (new TimedLogEvent("Initialize game"))
            {
                // Switch mode
                CurrentState = GameState.InGame;

                // Clean previous presenter
                //CleanupPresenter();

                // Load game scene
                CurrentPresenter = new InGamePresenter(Engine, CurrentSession.Universe);
                CurrentPresenter.Initialize();

                // Activate new view
                CurrentSession.Update(0, 0);
                AddInputReceiver((InteractivePresenter)CurrentPresenter);
                CurrentPresenter.HookIn();
                if (Settings.Current.Graphics.Fading) Engine.FadeIn();

                // Show game GUI
                GuiManager.Reset();
                LoadDialog("InGame/HUD");
            }

            using (new TimedLogEvent("Clean caches"))
            {
                Engine.Cache.Clean();
                GC.Collect();
            }

            Loading = false;
        }
        #endregion

        #region Initialize Modify
        /// <summary>
        /// Creates the <see cref="CurrentPresenter"/> and configures the GUI for live modification mode
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect", Justification = "This method is only called after level-loads, when a lot of long-lived objects have turned into garbage")]
        private void InitializeModifyMode()
        {
            Loading = true;

            using (new TimedLogEvent("Initialize game (in modify mode)"))
            {
                // Switch mode
                CurrentState = GameState.Modify;

                // Clean previous presenter
                //CleanupPresenter();

                // Load game scene
                CurrentPresenter = new InGamePresenter(Engine, CurrentSession.Universe);
                CurrentPresenter.Initialize();

                // Activate new view
                CurrentSession.Update(0, 0);
                AddInputReceiver((InteractivePresenter)CurrentPresenter);
                CurrentPresenter.HookIn();
                if (Settings.Current.Graphics.Fading) Engine.FadeIn();

                // Show game GUI
                GuiManager.Reset();
                LoadDialog("InGame/HUD_Modify");
            }

            using (new TimedLogEvent("Clean caches"))
            {
                Engine.Cache.Clean();
                GC.Collect();
            }

            Loading = false;
        }
        #endregion

        //--------------------//

        #region Cleanup
        /// <summary>
        /// <see cref="Presenter.HookOut"/> and disposes the <see cref="CurrentPresenter"/> (unless it is the <see cref="_menuPresenter"/>)
        /// </summary>
        private void CleanupPresenter()
        {
            if (CurrentPresenter != null)
            {
                var interactivePresenter = CurrentPresenter as InteractivePresenter;
                if (interactivePresenter != null) RemoveInputReceiver(interactivePresenter);

                CurrentPresenter.HookOut();

                // Don't dispose the _menuPresenter, since it will be reused for fast switching
                if (CurrentPresenter != _menuPresenter) CurrentPresenter.Dispose();
            }
        }
        #endregion
    }
}

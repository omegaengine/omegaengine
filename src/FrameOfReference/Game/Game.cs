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
using System.Drawing;
using System.IO;
using FrameOfReference.Presentation;
using FrameOfReference.Properties;
using FrameOfReference.World;
using FrameOfReference.World.Config;
using LuaInterface;
using NanoByte.Common;
using NanoByte.Common.Storage.SlimDX;
using OmegaEngine;
using OmegaGUI;

namespace FrameOfReference
{
    /// <summary>
    /// Represents a running instance of the game
    /// </summary>
    public partial class Game : GameBase
    {
        #region Variables
        private Universe _menuUniverse;
        private MenuPresenter _menuPresenter;
        #endregion

        #region Properties
        /// <summary>
        /// Manages all GUI dialogs displayed in the game
        /// </summary>
        public GuiManager GuiManager { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new game instance
        /// </summary>
        public Game() : base(GeneralSettings.AppName, Resources.Icon, Resources.Loading, false)
        {}
        #endregion

        //--------------------//

        #region Start
        /// <inheritdoc/>
        [LuaHide]
        public override void Run()
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            #region Initialize
            Log.Info("Start game...");

            if (Settings.Current.Display.Fullscreen)
            { // Fullscreen mode (initially fake, will switch after loading is complete)
                Log.Info("... in fake fullscreen mode");
                ToFullscreen();
            }
            else
            { // Windowed mode
                Log.Info("... in windowed mode");
                ToWindowed(Settings.Current.Display.WindowSize);

                // Validate window size before continuing
                Settings.Current.Display.WindowSize = Form.ClientSize;
            }
            #endregion

            // Will return after the game has finished (is exiting)
            base.Run();

            // Auto-save session for later resuming
            if (CurrentSession != null)
            {
                try
                {
                    SaveSavegame("Resume");
                }
                catch (IOException ex)
                {
                    // Only log, don't warn user when auto-save fails
                    Log.Warn("Failed to save game session to user profile: " + ex.Message);
                }
            }
        }
        #endregion

        #region Apply changed settings
        /// <summary>
        /// Called when <see cref="ControlsSettings.Changed"/>
        /// </summary>
        private void ApplyControlsSettings()
        {
            MouseInputProvider.InvertMouse = Settings.Current.Controls.InvertMouse;
        }

        /// <summary>
        /// Called when <see cref="SoundSettings.Changed"/>
        /// </summary>
        private void ApplySoundSettings()
        {
            if (!Settings.Current.Sound.PlayMusic) Engine.Music.Stop(true);
        }
        #endregion

        //--------------------//

        #region Debug
        /// <inheritdoc/>
        [LuaHide]
        public override void Debug()
        {
            // Exit fullscreen mode gracefully
            Settings.Current.Display.Fullscreen = false;

            base.Debug();
        }
        #endregion

        #region Lua references
        /// <inheritdoc/>
        [LuaHide]
        public override void SetupLua(Lua lua)
        {
            #region Sanity checks
            if (lua == null) throw new ArgumentNullException("lua");
            #endregion

            base.SetupLua(lua);

            LuaRegistrationHelper.Enumeration<GameState>(lua);

            // Make methods globally accessible (without prepending the class name)
            LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(Program));
            LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(Settings));
            LuaRegistrationHelper.TaggedInstanceMethods(lua, GuiManager);

            lua["Settings"] = Settings.Current;
            lua["State"] = CurrentState;
            lua["Session"] = CurrentSession;
            lua["Presenter"] = CurrentPresenter;
            lua["Universe"] = CurrentPresenter.Universe;

            // Boolean flag to indicate if the game is running a mod
            lua["IsMod"] = (ContentManager.ModDir != null);
        }
        #endregion

        #region Dialog-control helper methods for Lua
        /// <summary>
        /// Loads and displays a new dialog.
        /// </summary>
        /// <param name="name">The XML file to load from.</param>
        /// <returns>The newly created dialog.</returns>
        [LuaGlobal(Description = "Loads and displays a new dialog.")]
        public DialogRenderer LoadDialog(string name)
        {
            var dialogRenderer = new DialogRenderer(GuiManager, name + ".xml", new Point(25, 25));
            SetupLua(dialogRenderer.Lua);
            dialogRenderer.Show();
            Engine.Render(0);
            return dialogRenderer;
        }

        /// <summary>
        /// Loads and displays a new modal (exclusivly focused) dialog.
        /// </summary>
        /// <param name="name">The XML file to load from.</param>
        /// <returns>The newly created dialog.</returns>
        [LuaGlobal(Description = "Loads and displays a new modal (exclusivly focused) dialog.")]
        public DialogRenderer LoadModalDialog(string name)
        {
            var dialogRenderer = new DialogRenderer(GuiManager, name + ".xml", new Point(25, 25));
            SetupLua(dialogRenderer.Lua);
            dialogRenderer.ShowModal();
            Engine.Render(0);
            return dialogRenderer;
        }

        /// <summary>
        /// Loads a new exclusive displayed splash-screen dialog.
        /// </summary>
        /// <param name="name">The XML file to load from</param>
        /// <returns>The newly created dialog.</returns>
        /// <remarks>Calling this method will close all other <see cref="DialogRenderer"/>s.</remarks>
        [LuaGlobal(Description = "Loads a new exclusive displayed splash-screen dialog.")]
        public DialogRenderer LoadSplashDialog(string name)
        {
            GuiManager.CloseAll();
            return LoadDialog(name);
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            try
            {
                // Dispose presenters
                if (_menuPresenter != null) _menuPresenter.Dispose();
                if (CurrentPresenter != null) CurrentPresenter.Dispose();

                // Shutdown GUI system
                if (GuiManager != null) GuiManager.Dispose();

                // Remove settings update hooks
                Settings.Current.General.Changed -= Program.UpdateLocale;
                Settings.Current.Controls.Changed -= ApplyControlsSettings;
                Settings.Current.Display.Changed -= ResetEngine;
                Settings.Current.Graphics.Changed -= ApplyGraphicsSettings;
                Settings.Current.Sound.Changed -= ApplySoundSettings;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}

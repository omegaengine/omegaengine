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
using System.IO;
using FrameOfReference.Presentation;
using FrameOfReference.Presentation.Config;
using FrameOfReference.Properties;
using FrameOfReference.World;
using LuaInterface;
using NanoByte.Common;
using OmegaEngine;
using OmegaEngine.Foundation.Storage;
using OmegaGUI;

namespace FrameOfReference;

/// <summary>
/// Represents a running instance of the game
/// </summary>
public partial class Game(Settings settings)
    : GameBase(Universe.AppName, Resources.Icon, Resources.Loading)
{
    private Universe? _menuUniverse;
    private MenuPresenter? _menuPresenter;

    /// <summary>
    /// Manages all GUI dialogs displayed in the game
    /// </summary>
    public GuiManager GuiManager { get; private set; } = null!;

    /// <inheritdoc/>
    [LuaHide]
    public override void Run()
    {
        if (Disposed) throw new ObjectDisposedException(ToString());

        Log.Info("Start game...");

        if (settings.Display.Fullscreen)
        { // Fullscreen mode (initially fake, will switch after loading is complete)
            Log.Info("... in fake fullscreen mode");
            ToFullscreen();
        }
        else
        { // Windowed mode
            Log.Info("... in windowed mode");
            ToWindowed(settings.Display.WindowSize);

            // Validate window size before continuing
            settings.Display.WindowSize = Form.ClientSize;
        }

        // Will return after the game has finished (is exiting)
        base.Run();

        // Auto-save session for later resuming
        if (CurrentSession != null && CurrentSession.TimeWarpFactor != 0)
        {
            try
            {
                SaveSavegame("Resume");
            }
            catch (IOException ex)
            {
                // Only log, don't warn user when auto-save fails
                Log.Warn($"Failed to save game session to user profile: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Called when <see cref="ControlsSettings.Changed"/>
    /// </summary>
    private void ApplyControlsSettings()
    {
        MouseInputProvider.InvertMouse = settings.Controls.InvertMouse;
    }

    /// <inheritdoc/>
    [LuaHide]
    public override void Debug()
    {
        // Exit fullscreen mode gracefully
        settings.Display.Fullscreen = false;

        base.Debug();
    }

    /// <inheritdoc/>
    [LuaHide]
    public override Lua NewLua()
    {
        var lua = base.NewLua();

        LuaRegistrationHelper.Enumeration<GameState>(lua);

        // Make methods globally accessible (without prepending the class name)
        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(Program));
        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(Settings));
        LuaRegistrationHelper.TaggedInstanceMethods(lua, GuiManager);

        lua["Settings"] = settings;
        lua["State"] = CurrentState;
        lua["Session"] = CurrentSession;
        lua["Presenter"] = CurrentPresenter ?? throw new InvalidOperationException($"{nameof(Presenter)} not set yet.");
        lua["Universe"] = CurrentPresenter.Universe;

        // Boolean flag to indicate if the game is running a mod
        lua["IsMod"] = (ContentManager.ModDir != null);

        return lua;
    }

    /// <summary>
    /// Loads and displays a new dialog.
    /// </summary>
    /// <param name="name">The XML file to load from.</param>
    /// <returns>The newly created dialog.</returns>
    [LuaGlobal(Description = "Loads and displays a new dialog.")]
    public DialogRenderer LoadDialog(string name)
    {
        var dialogRenderer = new DialogRenderer(GuiManager, $"{name}.xml", location: new(25, 25), lua: NewLua());
        dialogRenderer.Show();
        Engine.Render(0);
        return dialogRenderer;
    }

    /// <summary>
    /// Loads and displays a new modal (exclusively focused) dialog.
    /// </summary>
    /// <param name="name">The XML file to load from.</param>
    /// <returns>The newly created dialog.</returns>
    [LuaGlobal(Description = "Loads and displays a new modal (exclusivly focused) dialog.")]
    public DialogRenderer LoadModalDialog(string name)
    {
        var dialogRenderer = new DialogRenderer(GuiManager, $"{name}.xml", location: new(25, 25), lua: NewLua());
        dialogRenderer.ShowModal();
        Engine.Render(0);
        return dialogRenderer;
    }

    /// <summary>
    /// Loads a new exclusively displayed splash-screen dialog.
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

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                // Dispose presenters
                _menuPresenter?.Dispose();
                CurrentPresenter?.Dispose();

                // Shutdown GUI system
                GuiManager.Dispose();

                // Remove settings update hooks
                settings.General.Changed -= Program.UpdateLocale;
                settings.Controls.Changed -= ApplyControlsSettings;
                settings.Display.Changed -= ResetEngine;
                settings.Graphics.Changed -= ApplyGraphicsSettings;
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
}

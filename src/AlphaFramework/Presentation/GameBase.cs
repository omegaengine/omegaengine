/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using AlphaFramework.Presentation.Config;
using LuaInterface;
using NanoByte.Common;
using OmegaEngine;
using OmegaEngine.Foundation.Storage;
using OmegaGUI;

namespace AlphaFramework.Presentation;

/// <summary>
/// Base class for building a game using AlphaFramework. Handles basic engine and GUI setup.
/// </summary>
/// <param name="settings">Settings for the game</param>
/// <param name="name">The name of the application for the title bar</param>
/// <param name="icon">The icon of the application for the title bar</param>
/// <param name="background">A background image for the window while loading</param>
/// <param name="stretch">Stretch <paramref name="background"/> to fit the screen? (<c>false</c> will center it instead)</param>
public abstract class GameBase(SettingsBase settings, string name, Icon? icon = null, Image? background = null, bool stretch = false)
    : RenderHost(name, icon, background, stretch)
{
    private GuiManager? _guiManager;

    /// <summary>
    /// Manages all GUI dialogs displayed in the game
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="Run"/> has not been called yet.</exception>
    [LuaHide]
    public GuiManager GuiManager => _guiManager ?? throw new InvalidOperationException($"{nameof(Run)} has not been called yet.");

    /// <inheritdoc/>
    protected override bool Initialize()
    {
        // Run the predefined init-steps first
        if (!base.Initialize()) return false;

        // Settings update hooks
        settings.Display.Changed += ResetEngine;
        settings.Graphics.Changed += ApplyGraphicsSettings;

        Form.ResizeEnd += delegate
        {
            if (!settings.Display.Fullscreen)
                settings.Display.WindowSize = Form.ClientSize;
        };

        using (new TimedLogEvent("Initialize GUI"))
        {
            // Initialize GUI subsystem
            _guiManager = new(Engine);
            Form.WindowMessage += _guiManager.OnMsgProc;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                // Shutdown GUI system
                _guiManager?.Dispose();

                // Remove settings update hooks
                settings.Display.Changed -= ResetEngine;
                settings.Graphics.Changed -= ApplyGraphicsSettings;
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

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

        base.Run();
    }

    /// <inheritdoc/>
    protected override void Render(double elapsedTime)
    {
        // Note: Doesn't call base methods

        // Check if we are currently in fake fullscreen mode (just a big window)
        if (Fullscreen && !Engine.Config.Fullscreen)
        {
            // Now switch the Direct3D device to real fullscreen mode
            Log.Info("Switch to real fullscreen mode");
            ResetEngine();
        }

        double elapsedGameTime = GetElapsedGameTime(elapsedTime);
        Engine.Render(elapsedGameTime);
    }

    /// <inheritdoc/>
    protected override void ResetEngine()
    {
        if (settings.Display.Fullscreen)
        { // Fullscreen
            ToFullscreen();
        }
        else
        { // Windowed
            ToWindowed(settings.Display.WindowSize);

            // Validate window size before continuing
            if (Form.ClientSize != settings.Display.WindowSize)
            {
                settings.Display.WindowSize = Form.ClientSize;
                return;
            }
        }

        base.ResetEngine();
    }

    /// <inheritdoc/>
    protected override EngineConfig BuildEngineConfig(bool fullscreen)
        => settings.Display.ToEngineConfig(fullscreen ? null : Form.ClientSize);

    /// <inheritdoc/>
    protected override void ApplyGraphicsSettings()
        => settings.Graphics.ApplyTo(Engine);

    /// <summary>
    /// Determines the amount of elapsed game time from the amount of elapsed real time.
    /// </summary>
    protected virtual double GetElapsedGameTime(double elapsedTime) => elapsedTime;

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

        // Make methods globally accessible (without prepending the class name)
        LuaRegistrationHelper.TaggedInstanceMethods(lua, GuiManager);

        lua["Game"] = this;
        lua["Settings"] = settings;
        lua["IsMod"] = ContentManager.ModDir != null;

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
        Engine.Render(elapsedGameTime: 0);
        return dialogRenderer;
    }

    /// <summary>
    /// Loads and displays a new modal (exclusively focused) dialog.
    /// </summary>
    /// <param name="name">The XML file to load from.</param>
    /// <returns>The newly created dialog.</returns>
    [LuaGlobal(Description = "Loads and displays a new modal (exclusively focused) dialog.")]
    public DialogRenderer LoadModalDialog(string name)
    {
        var dialogRenderer = new DialogRenderer(GuiManager, $"{name}.xml", location: new(25, 25), lua: NewLua());
        dialogRenderer.ShowModal();
        Engine.Render(elapsedGameTime: 0);
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
}

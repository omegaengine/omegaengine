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
using JetBrains.Annotations;
using NLua;
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
        settings.General.Changed += ApplyGeneralSettings;
        settings.Controls.Changed += ApplyControlsSettings;
        settings.Display.Changed += ResetEngine;
        settings.Graphics.Changed += ApplyGraphicsSettings;
        settings.Audio.Changed += ApplyAudioSettings;

        ApplyGraphicsSettings();
        ApplyAudioSettings();

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
                settings.General.Changed -= ApplyGeneralSettings;
                settings.Controls.Changed -= ApplyControlsSettings;
                settings.Display.Changed -= ResetEngine;
                settings.Graphics.Changed -= ApplyGraphicsSettings;
                settings.Audio.Changed -= ApplyAudioSettings;
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

        Engine.Music.Update();
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

    /// <summary>
    /// Called when <see cref="GeneralSettings.Changed"/>.
    /// </summary>
    protected virtual void ApplyGeneralSettings() {}

    /// <summary>
    /// Called when <see cref="ControlsSettings.Changed"/>.
    /// </summary>
    protected virtual void ApplyControlsSettings()
    {
        MouseInputProvider.InvertMouse = settings.Controls.InvertMouse;
        MouseInputProvider.CursorSensitivity = settings.Controls.MouseSensitivity;
    }

    /// <summary>
    /// Called when graphics settings from an external source need to be applied to the <see cref="Engine"/>
    /// </summary>
    protected virtual void ApplyGraphicsSettings()
        => settings.Graphics.ApplyTo(Engine);

    /// <summary>
    /// Called when audio settings from an external source need to be applied to the <see cref="Engine"/>
    /// </summary>
    protected virtual void ApplyAudioSettings()
        => settings.Audio.ApplyTo(Engine);

    /// <summary>
    /// Determines the amount of elapsed game time from the amount of elapsed real time.
    /// </summary>
    protected virtual double GetElapsedGameTime(double elapsedTime) => elapsedTime;

    /// <inheritdoc/>
    protected override void ShowDebugConsole()
    {
        // Exit fullscreen mode gracefully
        settings.Display.Fullscreen = false;

        base.ShowDebugConsole();
    }

    /// <inheritdoc/>
    [LuaHide]
    public override void BindLua(Lua lua)
    {
        base.BindLua(lua);

        // Make methods globally accessible (without prepending the class name)
        LuaRegistrationHelper.TaggedInstanceMethods(lua, GuiManager);

        lua["Game"] = this;
        lua["Settings"] = settings;
        lua["IsMod"] = ContentManager.ModDir != null;
    }

    /// <summary>
    /// Loads and displays a new dialog.
    /// </summary>
    /// <param name="name">The XML file to load from (without the <c>.xml</c> file ending).</param>
    /// <param name="options">Controls how the dialog is displayed.</param>
    /// <param name="args">Additional state to expose to the dialog's Lua instance as <c>Args</c>. A Lua table (copied) or a .NET object (by reference).</param>
    /// <returns>The newly created dialog.</returns>
    public DialogPresenter LoadDialog(string name, DialogOptions options, object? args = null)
    {
        if (options.Splash) GuiManager.CloseAll();

        var dialog = new DialogPresenter(GuiManager, $"{name}.xml", location: options.Location ?? new(25, 25), lua: NewLua(), args);
        if (options.Centered)
            dialog.Render.Location = new((Engine.RenderSize.Width - dialog.Render.Width) / 2, (Engine.RenderSize.Height - dialog.Render.Height) / 2);

        if (options.Modal) dialog.ShowModal();
        else dialog.Show();

        Engine.Render(elapsedGameTime: 0);
        return dialog;
    }

    /// <summary>
    /// Loads and displays a new dialog.
    /// </summary>
    /// <param name="name">The XML file to load from (without the <c>.xml</c> file ending).</param>
    /// <param name="options">Controls how the dialog is displayed. A Lua table that can be parsed into a <see cref="DialogOptions"/> instance (e.g. <c>{Modal = true}</c>).</param>
    /// <param name="args">Additional state to expose to the dialog's Lua instance as <c>Args</c>. A Lua table (copied).</param>
    /// <returns>The newly created dialog.</returns>
    [LuaMember, UsedImplicitly]
    public DialogPresenter LoadDialog(string name, object? options = null, object? args = null)
        => LoadDialog(name, DialogOptions.Parse(options), args);
}

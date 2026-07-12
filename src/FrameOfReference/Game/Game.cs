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
using AlphaFramework.Presentation;
using FrameOfReference.Presentation.Config;
using FrameOfReference.Properties;
using FrameOfReference.States;
using FrameOfReference.World;
using FrameOfReference.World.Templates;
using JetBrains.Annotations;
using NLua;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Storage;
using OmegaEngine.Input;

namespace FrameOfReference;

/// <summary>
/// Represents a running instance of the game.
/// </summary>
public class Game(Settings settings)
    : GameBase(settings, Constants.AppName, Resources.Icon, Resources.Loading)
{
    private IGameState? _state;

    private MainMenu? _menuState;
    private MainMenu MenuState => _menuState ??= new(this, GetMenuUniverse());

    private Session? _resumeSession;

    [UsedImplicitly]
    public bool CanResume => _resumeSession != null;

    /// <inheritdoc/>
    protected override bool Initialize()
    {
        if (!base.Initialize()) return false;

        using var _ = new TimedLogEvent("Game startup");
        UpdateStatus(Resources.StatusLoading);

        EntityTemplate.LoadAll();
        TerrainTemplate.LoadAll();

        if (Arguments.GetOption("map") is {} map)
            LoadMap(map);
        else if (Arguments.GetOption("savegame") is {} savegame)
        {
            try
            {
                new Savegames(this).Load(savegame);
            }
            #region Error handling
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
            {
                Msg.Inform(Form, ex.Message, MsgSeverity.Error);
                return false;
            }
            #endregion
        }
        else if (Arguments.GetOption("modify") is {} modify)
            ModifyMap(modify);
        else if (Arguments.HasOption("benchmark"))
        {
            TransitionTo(new Benchmark(this, onComplete: path =>
            {
                Form.Visible = false;
                Msg.Inform(null, $"Please upload the file '{path}'.", MsgSeverity.Info);
                Exit();
            }));
        }
        else
        {
            _resumeSession = Savegames.LoadFromResume();
            SwitchToMenu();
        }

        return true;
    }

    /// <summary>
    /// Switches to the main menu.
    /// </summary>
    [UsedImplicitly]
    public void SwitchToMenu()
        => TransitionTo(MenuState);

    /// <summary>
    /// Loads a game map and switches to in-game mode.
    /// </summary>
    [UsedImplicitly]
    public void LoadMap(string name)
        => SwitchToInGame(new Session(GetMap(name)));

    /// <summary>
    /// Switches to in-game mode using the resume session.
    /// </summary>
    [UsedImplicitly]
    public void ResumeGame()
        => SwitchToInGame(_resumeSession ?? throw new InvalidOperationException(Resources.NoSessionLoaded));

    /// <summary>
    /// Switches to in-game mode using the provided <paramref name="session"/>.
    /// </summary>
    internal void SwitchToInGame(Session session)
    {
        _resumeSession = session;
        TransitionTo(new InGame(this, session));
    }

    /// <summary>
    /// Loads a game map and switches to live modification mode.
    /// </summary>
    [UsedImplicitly]
    public void ModifyMap(string name)
        => SwitchToModify(new Session(GetMap(name)));

    private void SwitchToModify(Session session)
    {
        _resumeSession = session;
        TransitionTo(new Modify(this, session));
    }

    private void TransitionTo(IGameState newState)
    {
        var oldState = _state;
        oldState?.Exit();

        _state = newState;
        _state.Enter();
        RefreshDebugConsole();

        if (oldState != _menuState) oldState?.Dispose();
    }

    private static Universe GetMenuUniverse()
        => GetMap(Arguments.GetOption("menu") ?? "Menu");

    private static Universe GetMap(string name)
        => name.EndsWith(Constants.MapFileExt, StringComparison.OrdinalIgnoreCase)
            ? Universe.Load(Path.Combine(Locations.InstallBase, name))
            : Universe.FromContent(name + Constants.MapFileExt);

    /// <inheritdoc/>
    protected override void ApplyGeneralSettings() => Program.UpdateLocale();

    /// <inheritdoc/>
    protected override double GetElapsedGameTime(double elapsedTime)
        => _state?.GetElapsedGameTime(elapsedTime) ?? elapsedTime;

    /// <inheritdoc/>
    [LuaHide]
    public override void BindLua(Lua lua)
    {
        base.BindLua(lua);

        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(Program));
        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(Settings));

        lua["Game"] = this;
        _state?.BindLua(lua);
    }

    /// <inheritdoc/>
    [LuaHide]
    public override void Run()
    {
        MouseInputProvider.Scheme = MouseInputScheme.Planar;
        base.Run();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                _state?.Exit();
                _state?.Dispose();
                if (_state != _menuState) _menuState?.Dispose();
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
}

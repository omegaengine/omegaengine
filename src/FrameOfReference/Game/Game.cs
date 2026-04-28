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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlphaFramework.Presentation;
using FrameOfReference.Presentation;
using FrameOfReference.Presentation.Config;
using FrameOfReference.Properties;
using FrameOfReference.World;
using FrameOfReference.World.Templates;
using JetBrains.Annotations;
using LuaInterface;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Controls;
using NanoByte.Common.Storage;
using OmegaEngine;
using OmegaEngine.Input;

namespace FrameOfReference;

/// <summary>
/// Represents a running instance of the game
/// </summary>
public class Game(Settings settings)
    : GameBase(settings, Constants.AppName, Resources.Icon, Resources.Loading)
{
    private GameState _currentState;
    private Session? _currentSession;
    private Presenter? _currentPresenter;

    private Universe? _menuUniverse;
    private MenuPresenter? _menuPresenter;

    /// <inheritdoc/>
    protected override bool Initialize()
    {
        if (!base.Initialize()) return false;

        UpdateStatus(Resources.LoadingGraphics);
        using (new TimedLogEvent("Load graphics"))
        {
            EntityTemplate.LoadAll();
            TerrainTemplate.LoadAll();

            // Handle command-line arguments
            if (Arguments.GetOption("map") is {} map)
            { // Load command-line map
                LoadMap(map);
            }
            else if (Arguments.GetOption("modify") is {} modify)
            { // Load command-line map for modification
                ModifyMap(modify);
            }
            else if (Arguments.HasOption("benchmark"))
            { // Run automatic benchmark
                StartBenchmark();
            }
            else
            { // Load main menu
                PreloadPreviousSession();
                LoadMenu(GetMenuMap());
            }
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
                // Dispose presenters
                _menuPresenter?.Dispose();
                _currentPresenter?.Dispose();
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
        MouseInputProvider.Scheme = MouseInputScheme.Planar;
        base.Run();

        // Save before exit
        SaveSavegame("Resume");
    }

    /// <inheritdoc/>
    protected override void ApplyGeneralSettings() => Program.UpdateLocale();

    /// <inheritdoc/>
    protected override double GetElapsedGameTime(double elapsedTime)
        => _currentState switch
        {
            // Time passes as defined by the session
            GameState.InGame or GameState.Modify => _currentSession?.Update(elapsedTime) ?? elapsedTime,

            // Time passes very slowly and does not affect session
            GameState.Pause => elapsedTime / 10,

            // Time passes normally, but there is no session
            _ => elapsedTime
        };

    /// <inheritdoc/>
    [LuaHide]
    public override Lua NewLua()
    {
        var lua = base.NewLua();

        LuaRegistrationHelper.Enumeration<GameState>(lua);

        // Make methods globally accessible (without prepending the class name)
        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(Program));
        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(Settings));

        lua["State"] = _currentState;
        lua["Session"] = _currentSession;
        lua["Universe"] = _currentSession?.Universe;
        lua["Presenter"] = _currentPresenter ?? throw new InvalidOperationException($"{nameof(Presenter)} not set yet.");

        return lua;
    }

    /// <summary>
    /// Switches to the main menu
    /// </summary>
    /// <remarks>Loading will take a while on first call, subsequent calls will be very fast, because <see cref="_menuUniverse"/> is preserved</remarks>
    [UsedImplicitly]
    public void SwitchToMenu()
    {
        SaveSavegame("Resume");

        // Handle cases where the main menu was bypassed on startup
        if (_menuUniverse == null)
        {
            LoadMenu(GetMenuMap());
            return;
        }

        // Prevent unnecessary loading
        if (_currentState == GameState.Menu) return;

        CleanupPresenter();
        InitializeMenuMode();
    }

    /// <summary>
    /// Switches the game to in-game mode
    /// </summary>
    /// <remarks>Loading may take a while, subsequent calls will be a bit faster because the <see cref="Engine"/> cache will still be hot.</remarks>
    [UsedImplicitly]
    public void SwitchToGame()
    {
        if (_currentSession == null) throw new InvalidOperationException(Resources.NoSessionLoaded);

        // Prevent unnecessary loading
        if (_currentState == GameState.InGame) return;

        CleanupPresenter();
        InitializeGameMode();
    }

    /// <summary>
    /// Switches the game to map modification mode
    /// </summary>
    /// <remarks>Loading may take a while, subsequent calls will be a bit faster because the <see cref="Engine"/> cache will still be hot.</remarks>
    [UsedImplicitly]
    public void SwitchToModify()
    {
        if (_currentSession == null) throw new InvalidOperationException(Resources.NoSessionLoaded);

        // Prevent unnecessary loading
        if (_currentState == GameState.Modify) return;

        CleanupPresenter();
        InitializeModifyMode();
    }

    private GameState _stateBeforePause;

    /// <summary>
    /// Toggles between <see cref="GameState.InGame"/> and <see cref="GameState.Pause"/>
    /// </summary>
    [UsedImplicitly]
    public void TogglePause()
    {
        var interactivePresenter = _currentPresenter as InteractivePresenter;

        switch (_currentState)
        {
            case GameState.InGame or GameState.Modify:
                // Backup previous state
                _stateBeforePause = _currentState;

                _currentState = GameState.Pause;

                // Freeze the mouse interaction
                if (interactivePresenter != null) this.RemoveInputReceiver(interactivePresenter);

                // Dim down the screen
                _currentPresenter?.DimDown();

                // Show pause menu
                GuiManager.Reset();
                LoadDialog("PauseMenu");
                break;

            case GameState.Pause:
                // Restore previous state (usually GameState.InGame)
                _currentState = _stateBeforePause;

                // Dim screen back up
                _currentPresenter?.DimUp();

                // Restore the mouse interaction
                if (interactivePresenter != null) this.AddInputReceiver(interactivePresenter);

                // Restore the correct HUD
                GuiManager.Reset();
                if (_currentState == GameState.InGame) LoadDialog("InGame/HUD");
                else if (_currentState == GameState.Modify) LoadDialog("InGame/HUD_Modify");
                break;
        }
    }

    /// <summary>
    /// Loads the benchmark map into <see cref="_currentSession"/> and switches the <see cref="_currentState"/> to <see cref="GameState.Benchmark"/>
    /// </summary>
    private void StartBenchmark()
    {
        // Prevent unnecessary loading
        if (_currentState == GameState.Benchmark) return;

        using (new TimedLogEvent("Start benchmark"))
        {
            // Load map
            _currentSession = new(Universe.FromContent($"Benchmark{Constants.MapFileExt}"));

            // Switch mode
            _currentState = GameState.Benchmark;

            // Clean up any old stuff
            if (_currentPresenter != null)
            {
                if (_currentPresenter is InteractivePresenter interactivePresenter) this.RemoveInputReceiver(interactivePresenter);

                _currentPresenter.HookOut();
                if (_currentPresenter != _menuPresenter) _currentPresenter.Dispose();
            }

            // Load benchmark universe
            _currentPresenter = new BenchmarkPresenter(Engine,
                Universe.FromContent($"Benchmark{Constants.MapFileExt}"), path =>
                { // Callback for submitting the benchmark results
                    Form.Visible = false;
                    //if (Msg.Ask(Form, Resources.BenchmarkReady, MsgSeverity.Info, Resources.BenchmarkReadyContinue, Resources.BenchmarkReadyCancel))
                    //{
                    //    // ToDo: new Uri("https://omegaengine.de/benchmark-upload/?app=" + Constants.AppNameGrid)
                    //}
                    Msg.Inform(null, $"Please upload the file '{path}'.", MsgSeverity.Info);
                    Exit();
                });
            _currentPresenter.Initialize();

            // Note: Do not call before Presenter has been initialized
            _currentSession.Lua = NewLua();

            // Activate new view
            _currentPresenter.HookIn();
            if (settings.Graphics.Fading) Engine.FadeIn();

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

    /// <summary>
    /// Creates the <see cref="_menuPresenter"/> if necessary, sets it as the <see cref="_currentPresenter"/> and configures the GUI for the main menu
    /// </summary>
    private void InitializeMenuMode()
    {
        Loading = true;

        using (new TimedLogEvent("Initialize menu"))
        {
            // Switch mode
            _currentState = GameState.Menu;

            // Clean previous presenter
            //CleanupPresenter();

            // Load menu scene
            _menuPresenter ??= new(Engine, _menuUniverse!);
            _menuPresenter.Initialize();
            _currentPresenter = _menuPresenter;

            // Activate new view
            _currentPresenter.HookIn();
            if (settings.Graphics.Fading) Engine.FadeIn();

            // Show game GUI
            GuiManager.CloseAll();
            LoadDialog("MainMenu");
        }

        Loading = false;
    }

    /// <summary>
    /// Creates the <see cref="_currentPresenter"/> and configures the GUI for in-game mode
    /// </summary>
    private void InitializeGameMode()
    {
        if (_currentSession == null) throw new InvalidOperationException(Resources.NoSessionLoaded);

        Loading = true;

        using (new TimedLogEvent("Initialize game"))
        {
            // Switch mode
            _currentState = GameState.InGame;

            // Clean previous presenter
            CleanupPresenter();

            // Load game scene
            var presenter = new InGamePresenter(Engine, _currentSession.Universe);
            presenter.Initialize();
            _currentPresenter = presenter;

            // Activate new view
            this.AddInputReceiver(presenter);
            _currentPresenter.HookIn();
            if (settings.Graphics.Fading) Engine.FadeIn();

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

    /// <summary>
    /// Creates the <see cref="_currentPresenter"/> and configures the GUI for live modification mode
    /// </summary>
    private void InitializeModifyMode()
    {
        if (_currentSession == null) throw new InvalidOperationException(Resources.NoSessionLoaded);

        Loading = true;

        using (new TimedLogEvent("Initialize game (in modify mode)"))
        {
            // Switch mode
            _currentState = GameState.Modify;

            // Clean previous presenter
            CleanupPresenter();

            // Load game scene
            _currentPresenter = new InGamePresenter(Engine, _currentSession.Universe);
            _currentPresenter.Initialize();

            // Activate new view
            this.AddInputReceiver((InteractivePresenter)_currentPresenter);
            _currentPresenter.HookIn();
            if (settings.Graphics.Fading) Engine.FadeIn();

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

    /// <summary>
    /// <see cref="PresenterBase{TUniverse}.HookOut"/> and disposes the <see cref="_currentPresenter"/> (unless it is the <see cref="_menuPresenter"/>)
    /// </summary>
    private void CleanupPresenter()
    {
        if (_currentPresenter != null)
        {
            if (_currentPresenter is InteractivePresenter interactivePresenter) this.RemoveInputReceiver(interactivePresenter);

            _currentPresenter.HookOut();

            // Don't dispose the _menuPresenter, since it will be reused for fast switching
            if (_currentPresenter != _menuPresenter) _currentPresenter.Dispose();
        }
    }

    /// <returns>The name of the menu background map</returns>
    private static string GetMenuMap()
        => Arguments.GetOption("menu") ?? "Menu";

    /// <summary>
    /// Loads the auto-save into <see cref="_currentSession"/> for later usage
    /// </summary>
    private void PreloadPreviousSession()
    {
        try
        {
            LoadSavegame("Resume");
        }
        catch (FileNotFoundException)
        {
            Log.Info("No previous game session found");
        }
        catch (Exception ex)
        {
            Log.Warn($"Failed to restore previous game session: {ex.Message}");
            return;
        }
        Log.Info("Previous game session restored");
    }

    /// <summary>
    /// Loads a map into <see cref="_menuUniverse"/> and switches to <see cref="GameState.Menu"/>
    /// </summary>
    /// <param name="name">The name of the map to load</param>
    [UsedImplicitly]
    public void LoadMenu(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        _menuUniverse = name.EndsWith(
            // Does the name have file ending?
            Constants.MapFileExt, StringComparison.OrdinalIgnoreCase) ?
            // Real filename
            Universe.Load(Path.Combine(Locations.InstallBase, name)) :
            // Internal map name
            Universe.FromContent(name + Constants.MapFileExt);

        CleanupPresenter();
        if (_menuPresenter != null)
        { // Prevent the old presenter from being reused for fast switching
            _menuPresenter.Dispose();
            _menuPresenter = null;
        }
        InitializeMenuMode();
    }

    /// <summary>
    /// Loads a game map into <see cref="_currentSession"/> and switches to <see cref="GameState.InGame"/>
    /// </summary>
    /// <param name="name">The name of the map to load</param>
    [UsedImplicitly]
    public void LoadMap(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        _currentSession = new(
            // Does the name have file ending?
            name.EndsWith(Constants.MapFileExt, StringComparison.OrdinalIgnoreCase) ?
                // Real filename
                Universe.Load(Path.Combine(Locations.InstallBase, name)) :
                // Internal map name
                Universe.FromContent(name + Constants.MapFileExt));

        CleanupPresenter();
        InitializeGameMode();

        // Note: Do not call before Presenter has been initialized
        _currentSession.Lua = NewLua();
    }

    /// <summary>
    /// Loads a game map into <see cref="_currentSession"/> and switches the <see cref="_currentState"/> to <see cref="GameState.Modify"/>
    /// </summary>
    /// <param name="name">The name of the map to load</param>
    [UsedImplicitly]
    public void ModifyMap(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        _currentSession = new(
            // Does the name have file ending?
            name.EndsWith(Constants.MapFileExt, StringComparison.OrdinalIgnoreCase) ?
                // Real filename
                Universe.Load(Path.Combine(Locations.InstallBase, name)) :
                // Internal map name
                Universe.FromContent(name + Constants.MapFileExt));

        CleanupPresenter();
        InitializeModifyMode();

        // Note: Do not call before Presenter has been initialized
        _currentSession.Lua = NewLua();
    }

    /// <summary>
    /// Saves the <see cref="_currentSession"/> as a savegame stored in the user's profile.
    /// </summary>
    /// <param name="name">The name of the savegame to write.</param>
    [UsedImplicitly]
    public void SaveSavegame(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        // If we are currently in-game, then the camera position must be explicitly stored/updated
        if (_currentState is GameState.InGame or GameState.Pause)
            ((InGamePresenter)_currentPresenter!).PrepareSave();

        // Write to disk
        string path = Locations.GetSaveDataPath(Constants.AppName, isFile: true, name + Constants.SavegameFileExt);
        _currentSession?.Save(path);
    }

    /// <summary>
    /// Loads a savegame from user's profile to replace the <see cref="_currentSession"/>.
    /// </summary>
    /// <param name="name">The name of the savegame to load.</param>
    [UsedImplicitly]
    public void LoadSavegame(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        // Read from disk
        string path = Locations.GetSaveDataPath(Constants.AppName, isFile: true, name + Constants.SavegameFileExt);
        _currentSession = Session.Load(path);
        _currentSession.Lua = NewLua();
    }

    /// <summary>
    /// Lists the names of all stored <see cref="Session"/>s.
    /// </summary>
    [UsedImplicitly]
    public IEnumerable<string> GetSavegameNames()
    {
        var savegameDir = new DirectoryInfo(Locations.GetSaveDataPath(Constants.AppName, isFile: false));
        return savegameDir.GetFiles($"*{Constants.SavegameFileExt}")
                          .Select(x => x.Name[..^Constants.SavegameFileExt.Length])
                          .Except("Resume");
    }
}

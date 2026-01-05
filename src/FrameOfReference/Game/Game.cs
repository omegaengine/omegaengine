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
    /// <summary>
    /// The current state of the game
    /// </summary>
    [LuaHide]
    public GameState CurrentState { get; private set; }

    /// <summary>
    /// The current game session
    /// </summary>
    [LuaHide]
    public Session? CurrentSession { get; private set; }

    /// <summary>
    /// The currently active presenter
    /// </summary>
    [LuaHide]
    public Presenter? CurrentPresenter { get; private set; }

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
            if (Program.Args["map"] is {} map)
            { // Load command-line map
                LoadMap(map);
            }
            else if (Program.Args["modify"] is {} modify)
            { // Load command-line map for modification
                ModifyMap(modify);
            }
            else if (Program.Args.Contains("benchmark"))
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
                CurrentPresenter?.Dispose();
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

    /// <inheritdoc/>
    protected override void ApplyGeneralSettings() => Program.UpdateLocale();

    /// <inheritdoc />
    protected override void ApplyGraphicsSettings()
    {
        base.ApplyGraphicsSettings();
        if (CurrentPresenter != null) CurrentPresenter.View.Camera.FieldOfView = settings.Graphics.FieldOfView;
    }

    /// <inheritdoc/>
    protected override double GetElapsedGameTime(double elapsedTime)
        => CurrentState switch
        {
            // Time passes as defined by the session
            GameState.InGame or GameState.Modify => CurrentSession?.Update(elapsedTime) ?? elapsedTime,

            // Time passes very slowly and does not affect session
            GameState.Pause => elapsedTime / 10,

            // Time passes normally but there is no session
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

        lua["State"] = CurrentState;
        lua["Session"] = CurrentSession;
        lua["Presenter"] = CurrentPresenter ?? throw new InvalidOperationException($"{nameof(Presenter)} not set yet.");
        lua["Universe"] = CurrentPresenter.Universe;

        return lua;
    }

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
            LoadMenu(GetMenuMap());
            return;
        }

        // Prevent unnecessary loading
        if (CurrentState == GameState.Menu) return;

        CleanupPresenter();
        InitializeMenuMode();
    }

    /// <summary>
    /// Switches the game to in-game mode
    /// </summary>
    /// <remarks>If <see cref="CurrentState"/> is already <see cref="GameState.InGame"/>, nothing will happen.
    /// Loading may take a while, subsequent calls will be a bit faster because the <see cref="Engine"/> cache will still be hot</remarks>
    public void SwitchToGame()
    {
        if (CurrentSession == null) throw new InvalidOperationException(Resources.NoSessionLoaded);

        // Prevent unnecessary loading
        if (CurrentState == GameState.InGame) return;

        CleanupPresenter();
        InitializeGameMode();
    }

    /// <summary>
    /// Switches the game to map modification mode
    /// </summary>
    /// <remarks>If <see cref="CurrentState"/> is already <see cref="GameState.Modify"/>, nothing will happen.
    /// Loading may take a while, subsequent calls will be a bit faster because the <see cref="Engine"/> cache will still be hot</remarks>
    public void SwitchToModify()
    {
        if (CurrentSession == null) throw new InvalidOperationException(Resources.NoSessionLoaded);

        // Prevent unnecessary loading
        if (CurrentState == GameState.Modify) return;

        CleanupPresenter();
        InitializeModifyMode();
    }

    private GameState _stateBeforePause;

    /// <summary>
    /// Toggles <see cref="CurrentState"/> between <see cref="GameState.InGame"/> and <see cref="GameState.Pause"/>
    /// </summary>
    /// <remarks>When called while <see cref="CurrentState"/> is neither <see cref="GameState.InGame"/> nor <see cref="GameState.Pause"/> nothing happens</remarks>
    public void TogglePause()
    {
        var interactivePresenter = CurrentPresenter as InteractivePresenter;

        switch (CurrentState)
        {
            case GameState.InGame or GameState.Modify:
                // Backup previous state
                _stateBeforePause = CurrentState;

                CurrentState = GameState.Pause;

                // Freeze the mouse interaction
                if (interactivePresenter != null) this.RemoveInputReceiver(interactivePresenter);

                // Dim down the screen
                CurrentPresenter?.DimDown();

                // Show pause menu
                GuiManager.Reset();
                LoadDialog("PauseMenu");
                break;

            case GameState.Pause:
                // Restore previous state (usually GameState.InGame)
                CurrentState = _stateBeforePause;

                // Dim screen back up
                CurrentPresenter?.DimUp();

                // Restore the mouse interaction
                if (interactivePresenter != null) this.AddInputReceiver(interactivePresenter);

                // Restore the correct HUD
                GuiManager.Reset();
                if (CurrentState == GameState.InGame) LoadDialog("InGame/HUD");
                else if (CurrentState == GameState.Modify) LoadDialog("InGame/HUD_Modify");
                break;
        }
    }

    /// <summary>
    /// Loads the benchmark map into <see cref="CurrentSession"/> and switches the <see cref="CurrentState"/> to <see cref="GameState.Benchmark"/>
    /// </summary>
    /// <remarks>If <see cref="CurrentState"/> is <see cref="GameState.Benchmark"/>, nothing will happen</remarks>
    private void StartBenchmark()
    {
        // Prevent unnecessary loading
        if (CurrentState == GameState.Benchmark) return;

        using (new TimedLogEvent("Start benchmark"))
        {
            // Load map
            CurrentSession = new(Universe.FromContent($"Benchmark{Universe.FileExt}"));

            // Switch mode
            CurrentState = GameState.Benchmark;

            // Clean up any old stuff
            if (CurrentPresenter != null)
            {
                if (CurrentPresenter is InteractivePresenter interactivePresenter) this.RemoveInputReceiver(interactivePresenter);

                CurrentPresenter.HookOut();
                if (CurrentPresenter != _menuPresenter) CurrentPresenter.Dispose();
            }

            // Load benchmark universe
            CurrentPresenter = new BenchmarkPresenter(Engine,
                Universe.FromContent($"Benchmark{Universe.FileExt}"), path =>
                { // Callback for submitting the benchmark results
                    Form.Visible = false;
                    //if (Msg.Ask(Form, Resources.BenchmarkReady, MsgSeverity.Info, Resources.BenchmarkReadyContinue, Resources.BenchmarkReadyCancel))
                    //{
                    //    // ToDo: new Uri("https://omegaengine.de/benchmark-upload/?app=" + Constants.AppNameGrid)
                    //}
                    Msg.Inform(null, $"Please upload the file '{path}'.", MsgSeverity.Info);
                    Exit();
                });
            CurrentPresenter.Initialize();

            // Note: Do not call before Presenter has been initialized
            CurrentSession.Lua = NewLua();

            // Activate new view
            CurrentPresenter.HookIn();
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
            _menuPresenter ??= new(Engine, _menuUniverse!);
            _menuPresenter.Initialize();
            CurrentPresenter = _menuPresenter;

            // Activate new view
            CurrentPresenter.HookIn();
            if (settings.Graphics.Fading) Engine.FadeIn();

            // Show game GUI
            GuiManager.CloseAll();
            LoadDialog("MainMenu");
        }

        Loading = false;
    }

    /// <summary>
    /// Creates the <see cref="CurrentPresenter"/> and configures the GUI for in-game mode
    /// </summary>
    private void InitializeGameMode()
    {
        Loading = true;

        using (new TimedLogEvent("Initialize game"))
        {
            // Switch mode
            CurrentState = GameState.InGame;

            // Clean previous presenter
            CleanupPresenter();

            // Load game scene
            CurrentPresenter = new InGamePresenter(Engine, CurrentSession!.Universe);
            CurrentPresenter.Initialize();

            // Activate new view
            this.AddInputReceiver((InteractivePresenter)CurrentPresenter);
            CurrentPresenter.HookIn();
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
    /// Creates the <see cref="CurrentPresenter"/> and configures the GUI for live modification mode
    /// </summary>
    private void InitializeModifyMode()
    {
        Loading = true;

        using (new TimedLogEvent("Initialize game (in modify mode)"))
        {
            // Switch mode
            CurrentState = GameState.Modify;

            // Clean previous presenter
            CleanupPresenter();

            // Load game scene
            CurrentPresenter = new InGamePresenter(Engine, CurrentSession.Universe);
            CurrentPresenter.Initialize();

            // Activate new view
            this.AddInputReceiver((InteractivePresenter)CurrentPresenter);
            CurrentPresenter.HookIn();
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
    /// <see cref="PresenterBase{TUniverse}.HookOut"/> and disposes the <see cref="CurrentPresenter"/> (unless it is the <see cref="_menuPresenter"/>)
    /// </summary>
    private void CleanupPresenter()
    {
        if (CurrentPresenter != null)
        {
            if (CurrentPresenter is InteractivePresenter interactivePresenter) this.RemoveInputReceiver(interactivePresenter);

            CurrentPresenter.HookOut();

            // Don't dispose the _menuPresenter, since it will be reused for fast switching
            if (CurrentPresenter != _menuPresenter) CurrentPresenter.Dispose();
        }
    }

    /// <returns>The name of the menu background map</returns>
    private static string GetMenuMap()
        => Program.Args["menu"] ?? "Menu";

    /// <summary>
    /// Loads the auto-save into <see cref="CurrentSession"/> for later usage
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
    /// Loads a map into <see cref="_menuUniverse"/> and switches the <see cref="CurrentState"/> to <see cref="GameState.Menu"/>
    /// </summary>
    /// <param name="name">The name of the map to load</param>
    public void LoadMenu(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        _menuUniverse = name.EndsWith(
            // Does the name have file ending?
            Universe.FileExt, StringComparison.OrdinalIgnoreCase) ?
            // Real filename
            Universe.Load(Path.Combine(Locations.InstallBase, name)) :
            // Internal map name
            Universe.FromContent(name + Universe.FileExt);

        CleanupPresenter();
        if (_menuPresenter != null)
        { // Prevent the old presenter from being reused for fast switching
            _menuPresenter.Dispose();
            _menuPresenter = null;
        }
        InitializeMenuMode();
    }

    /// <summary>
    /// Loads a game map into <see cref="CurrentSession"/> and switches the <see cref="CurrentState"/> to <see cref="GameState.InGame"/>
    /// </summary>
    /// <param name="name">The name of the map to load</param>
    public void LoadMap(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        CurrentSession = new(
            // Does the name have file ending?
            name.EndsWith(Universe.FileExt, StringComparison.OrdinalIgnoreCase) ?
                // Real filename
                Universe.Load(Path.Combine(Locations.InstallBase, name)) :
                // Internal map name
                Universe.FromContent(name + Universe.FileExt));

        CleanupPresenter();
        InitializeGameMode();

        // Note: Do not call before Presenter has been initialized
        CurrentSession.Lua = NewLua();
    }

    /// <summary>
    /// Loads a game map into <see cref="CurrentSession"/> and switches the <see cref="CurrentState"/> to <see cref="GameState.Modify"/>
    /// </summary>
    /// <param name="name">The name of the map to load</param>
    public void ModifyMap(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        CurrentSession = new(
            // Does the name have file ending?
            name.EndsWith(Universe.FileExt, StringComparison.OrdinalIgnoreCase) ?
                // Real filename
                Universe.Load(Path.Combine(Locations.InstallBase, name)) :
                // Internal map name
                Universe.FromContent(name + Universe.FileExt));

        CleanupPresenter();
        InitializeModifyMode();

        // Note: Do not call before Presenter has been initialized
        CurrentSession.Lua = NewLua();
    }

    /// <summary>
    /// Saves the <see cref="CurrentSession"/> as a savegame stored in the user's profile.
    /// </summary>
    /// <param name="name">The name of the savegame to write.</param>
    public void SaveSavegame(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        // If we are currently in-game, then the camera position must be explicitly stored/updated
        if (CurrentState is GameState.InGame or GameState.Pause)
            ((InGamePresenter)CurrentPresenter!).PrepareSave();

        // Write to disk
        string path = Locations.GetSaveDataPath(Constants.AppName, isFile: true, name + Session.FileExt);
        CurrentSession?.Save(path);
    }

    /// <summary>
    /// Loads a savegame from user's profile to replace the <see cref="CurrentSession"/>.
    /// </summary>
    /// <param name="name">The name of the savegame to load.</param>
    public void LoadSavegame(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        // Read from disk
        string path = Locations.GetSaveDataPath(Constants.AppName, isFile: true, name + Session.FileExt);
        CurrentSession = Session.Load(path);
        CurrentSession.Lua = NewLua();
    }

    /// <summary>
    /// Lists the names of all stored <see cref="Session"/>s.
    /// </summary>
    public IEnumerable<string> GetSavegameNames()
    {
        var savegameDir = new DirectoryInfo(Locations.GetSaveDataPath(Constants.AppName, isFile: false));
        return savegameDir.GetFiles($"*{Session.FileExt}")
                          .Select(x => x.Name[..^Session.FileExt.Length])
                          .Except("Resume");
    }
}

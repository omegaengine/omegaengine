using AlphaFramework.Presentation;
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using Template.AlphaFramework.Presentation.Config;
using Template.AlphaFramework.States;
using Template.AlphaFramework.World;

namespace Template.AlphaFramework;

/// <summary>
/// Represents a running instance of the game.
/// </summary>
public class Game(Settings settings)
    : GameBase(settings, Constants.AppName)
{
    private IGameState? _state;

    private MainMenu? _menuState;
    private MainMenu MenuState => _menuState ??= new(this, CreateDemoUniverse());

    /// <inheritdoc/>
    protected override bool Initialize()
    {
        if (!base.Initialize()) return false;

        UpdateStatus("Loading");

        if (Arguments.GetOption("map") is {} map)
            LoadMap(map);
        else
            SwitchToMenu();

        return true;
    }

    /// <summary>
    /// Switches to the main menu.
    /// </summary>
    [UsedImplicitly]
    public void SwitchToMenu() => TransitionTo(MenuState);

    /// <summary>
    /// Starts a new game using an in-memory demo universe.
    /// </summary>
    [UsedImplicitly]
    public void NewGame() => SwitchToInGame(new Session(CreateDemoUniverse()));

    /// <summary>
    /// Loads a map (by content ID or file path) and switches to in-game mode.
    /// </summary>
    [UsedImplicitly]
    public void LoadMap(string name) => SwitchToInGame(new Session(GetMap(name)));

    private void SwitchToInGame(Session session) => TransitionTo(new InGame(this, session));

    private void TransitionTo(IGameState newState)
    {
        var oldState = _state;
        oldState?.Exit();

        _state = newState;
        _state.Enter();

        // Keep the menu state alive so its backdrop universe can be reused.
        if (oldState != _menuState) oldState?.Dispose();
    }

    /// <inheritdoc/>
    protected override double GetElapsedGameTime(double elapsedTime)
        => _state?.GetElapsedGameTime(elapsedTime) ?? elapsedTime;

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

    private static Universe GetMap(string name)
        => name.EndsWith(Constants.MapFileExt, StringComparison.OrdinalIgnoreCase)
            ? Universe.Load(Path.Combine(Locations.InstallBase, name))
            : Universe.FromContent(name + Constants.MapFileExt);

    /// <summary>
    /// Creates a minimal in-memory universe containing a single placeholder object.
    /// </summary>
    private static Universe CreateDemoUniverse()
    {
        var universe = new Universe();
        universe.Positionables.Add(new Entity {Name = "Object", Position = default});
        return universe;
    }
}

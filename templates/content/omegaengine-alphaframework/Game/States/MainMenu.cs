using OmegaEngine;
using Template.AlphaFramework.Presentation;
using Template.AlphaFramework.World;

namespace Template.AlphaFramework.States;

/// <summary>
/// State while the main menu is shown.
/// </summary>
/// <param name="game">The running game instance.</param>
/// <param name="universe">The universe used as a backdrop behind the menu.</param>
public sealed class MainMenu(Game game, Universe universe) : IGameState
{
    private readonly MenuPresenter _presenter = new(game.Engine, universe);

    /// <inheritdoc/>
    public void Enter()
    {
        _presenter.Initialize();
        _presenter.HookIn();
        game.Engine.FadeIn();

        game.GuiManager.CloseAll();
        game.LoadModalDialog("MainMenu");
    }

    /// <inheritdoc/>
    public void Exit() => _presenter.HookOut();

    /// <inheritdoc/>
    // The menu backdrop is not part of the simulation, so real time is not converted to game time.
    public double GetElapsedGameTime(double elapsedTime) => elapsedTime;

    /// <inheritdoc/>
    public void Dispose() => _presenter.Dispose();
}

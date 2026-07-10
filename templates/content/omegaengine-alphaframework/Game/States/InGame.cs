using OmegaEngine;
using Template.AlphaFramework.Presentation;
using Template.AlphaFramework.World;

namespace Template.AlphaFramework.States;

/// <summary>
/// State while a game session is active.
/// </summary>
/// <param name="game">The running game instance.</param>
/// <param name="session">The active game session.</param>
public sealed class InGame(Game game, Session session) : SessionStateBase(game, session)
{
    private readonly InGamePresenter _presenter = new(game.Engine, session.Universe);

    /// <inheritdoc/>
    public override void Enter()
    {
        _presenter.Initialize();
        game.AddInputReceiver(_presenter);
        _presenter.HookIn();
        game.Engine.FadeIn();

        game.GuiManager.Reset();
        game.LoadDialog("InGame");
    }

    /// <inheritdoc/>
    public override void Exit()
    {
        game.RemoveInputReceiver(_presenter);
        _presenter.HookOut();
    }

    /// <inheritdoc/>
    public override void Dispose() => _presenter.Dispose();
}

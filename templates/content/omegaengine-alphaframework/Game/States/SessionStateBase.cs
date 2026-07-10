using Template.AlphaFramework.World;

namespace Template.AlphaFramework.States;

/// <summary>
/// Common base for states that have an active <see cref="Session"/>.
/// </summary>
/// <param name="game">The running game instance.</param>
/// <param name="session">The active game session.</param>
public abstract class SessionStateBase(Game game, Session session) : IGameState
{
    /// <summary>The running game instance.</summary>
    protected readonly Game game = game;

    /// <inheritdoc/>
    public abstract void Enter();

    /// <inheritdoc/>
    public abstract void Exit();

    /// <inheritdoc/>
    public virtual double GetElapsedGameTime(double elapsedTime)
        => session.Update(elapsedTime);

    /// <inheritdoc/>
    public abstract void Dispose();
}

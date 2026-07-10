namespace Template.AlphaFramework.States;

/// <summary>
/// A distinct mode the <see cref="Game"/> can be in (e.g. main menu or in-game).
/// </summary>
public interface IGameState : IDisposable
{
    /// <summary>
    /// Called when this state becomes the active state.
    /// </summary>
    void Enter();

    /// <summary>
    /// Called when this state is no longer the active state.
    /// </summary>
    void Exit();

    /// <summary>
    /// Returns the amount of in-game time that corresponds to <paramref name="elapsedTime"/> real time.
    /// </summary>
    double GetElapsedGameTime(double elapsedTime);
}

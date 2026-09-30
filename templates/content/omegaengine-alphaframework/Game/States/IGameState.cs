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
    /// Advances the state by <paramref name="elapsedTime"/> seconds of real time. Called once per frame.
    /// </summary>
    /// <returns>The amount of game time that has passed.</returns>
    double Update(double elapsedTime);
}

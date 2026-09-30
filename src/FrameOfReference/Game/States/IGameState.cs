using System;
using NLua;

namespace FrameOfReference.States;

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
    /// Advances the state by <paramref name="elapsedTime"/> seconds of real time. Called once per frame before rendering.
    /// </summary>
    /// <returns>The amount of game time that has passed. Drives time-based visual effects.</returns>
    double Update(double elapsedTime);

    /// <summary>
    /// Registers state-specific values in the given Lua environment.
    /// </summary>
    void BindLua(Lua lua);
}

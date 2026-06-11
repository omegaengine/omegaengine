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
    /// Returns the amount of in-game time that corresponds to <paramref name="elapsedTime"/> real time.
    /// </summary>
    double GetElapsedGameTime(double elapsedTime);

    /// <summary>
    /// Registers state-specific values in the given Lua environment.
    /// </summary>
    void BindLua(Lua lua);
}

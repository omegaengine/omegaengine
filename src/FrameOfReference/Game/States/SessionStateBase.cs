using System;
using FrameOfReference.Presentation;
using FrameOfReference.World;
using LuaInterface;
using NanoByte.Common;

namespace FrameOfReference.States;

/// <summary>
/// Common base for states with an active <see cref="Session"/> and <see cref="Presenter"/>.
/// </summary>
public abstract class SessionStateBase(Game game, Session session) : IGameState
{
    protected readonly Game game = game;
    protected readonly Session session = session;

    /// <inheritdoc/>
    public abstract void Enter();

    /// <inheritdoc/>
    public abstract void Exit();

    /// <inheritdoc/>
    public virtual double GetElapsedGameTime(double elapsedTime)
        => session.Update(elapsedTime);

    /// <inheritdoc/>
    public virtual void BindLua(Lua lua)
    {
        lua["Session"] = session;
        lua["Universe"] = session.Universe;
    }

    protected void CleanCaches()
    {
        using (new TimedLogEvent("Clean caches"))
        {
            game.Engine.Cache.Clean();
            GC.Collect();
        }
    }

    protected void InitializeLua() => session.Lua = game.NewLua();

    /// <inheritdoc/>
    public abstract void Dispose();
}

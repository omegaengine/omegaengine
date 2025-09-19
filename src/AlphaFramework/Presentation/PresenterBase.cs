using System;
using System.IO;
using AlphaFramework.World;
using AlphaFramework.World.Positionables;
using LuaInterface;
using NanoByte.Common;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Renderables;

namespace AlphaFramework.Presentation;

/// <summary>
/// Uses the <see cref="Engine"/> to present an <see cref="IUniverse"/> game world.
/// </summary>
/// <typeparam name="TUniverse">The type of universe to present.</typeparam>
public abstract class PresenterBase<TUniverse>(Engine engine, TUniverse universe) : IDisposable
    where TUniverse : class, IUniverse
{
    /// <summary>
    /// The <see cref="Engine"/> reference to use for rendering operations
    /// </summary>
    protected readonly Engine Engine = engine;

    /// <summary>
    /// The engine scene containing the graphical representations of <see cref="Positionable{TCoordinates}"/>s
    /// </summary>
    protected readonly Scene Scene = new();

    /// <summary>
    /// The engine view used to display the <see cref="Scene"/>
    /// </summary>
    public required View View { get; init; }

    /// <summary>
    /// The game world to present.
    /// </summary>
    [LuaHide]
    public TUniverse Universe { get; } = universe;

    /// <summary>
    /// Was <see cref="Initialize"/> already called?
    /// </summary>
    protected bool Initialized { get; private set; }

    /// <summary>
    /// Generate <see cref="Renderable"/>s from the <see cref="Universe"/> and keeps everything in sync using events
    /// </summary>
    /// <exception cref="FileNotFoundException">A required <see cref="Asset"/> file could not be found.</exception>
    /// <exception cref="IOException">There was an error reading an <see cref="Asset"/> file.</exception>
    /// <exception cref="InvalidDataException">An <see cref="Asset"/> file contains invalid data.</exception>
    /// <remarks>Should be called before <see cref="HookIn"/> is used</remarks>
    public virtual void Initialize()
    {
        Initialized = true;
    }

    /// <summary>
    /// Hooks the <see cref="View"/> into <see cref="OmegaEngine.Engine.Views"/>
    /// </summary>
    /// <remarks>Will internally call <see cref="Initialize"/> first, if you didn't</remarks>
    public virtual void HookIn()
    {
        if (_disposed) throw new ObjectDisposedException(ToString());
        if (!Initialized) Initialize();

        Engine.Views.Add(View);
    }

    /// <summary>
    /// Hooks the <see cref="View"/> out of <see cref="OmegaEngine.Engine.Views"/>
    /// </summary>
    public virtual void HookOut() => Engine.Views.Remove(View);

    /// <summary>
    /// Dims in the screen down.
    /// </summary>
    public virtual void DimDown() => Engine.DimDown();

    /// <summary>
    /// Dims in the screen back up.
    /// </summary>
    public virtual void DimUp() => Engine.DimUp();

    private bool _disposed;

    /// <summary>
    /// Removes the <see cref="Universe"/> hooks setup by <see cref="Initialize"/> and disposes all created <see cref="View"/>s, <see cref="Scene"/>s, <see cref="PositionableRenderable"/>s, etc.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        Dispose(true);
        GC.SuppressFinalize(this);
        _disposed = true;
    }

    /// <inheritdoc/>
    ~PresenterBase()
    {
        Dispose(false);
    }

    /// <summary>
    /// To be called by <see cref="IDisposable.Dispose"/> and the object destructor.
    /// </summary>
    /// <param name="disposing"><c>true</c> if called manually and not by the garbage collector.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        { // This block will only be executed on manual disposal, not by Garbage Collection
            Log.Info("Disposing presenter");

            Scene.Dispose();
            View.Dispose();
        }
        else
        { // This block will only be executed on Garbage Collection, not by manual disposal
            Log.Error($"Forgot to call Dispose on {this}");
#if DEBUG
            throw new InvalidOperationException($"Forgot to call Dispose on {this}");
#endif
        }
    }
}

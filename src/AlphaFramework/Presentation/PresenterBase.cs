using System;
using AlphaFramework.World;
using AlphaFramework.World.Positionables;
using LuaInterface;
using NanoByte.Common;
using OmegaEngine;
using OmegaEngine.Foundation.Storage;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Renderables;

namespace AlphaFramework.Presentation;

/// <summary>
/// Uses the <see cref="Engine"/> to present an <see cref="IUniverse"/> game world.
/// </summary>
/// <typeparam name="TUniverse">The type of universe to present.</typeparam>
public abstract class PresenterBase<TUniverse>(Engine engine, TUniverse universe) : IPresenter<TUniverse>
    where TUniverse : UniverseBase
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
    public View View { get; protected init; }

    /// <inheritdoc/>
    [LuaHide]
    public TUniverse Universe { get; } = universe;

    /// <summary>
    /// Was <see cref="Initialize"/> already called?
    /// </summary>
    protected bool Initialized { get; private set; }

    /// <inheritdoc/>
    public virtual void Initialize()
    {
        Initialized = true;

        UpdateSkybox();
        Universe.SkyboxChanged += UpdateSkybox;
    }

    /// <inheritdoc/>
    public virtual void HookIn()
    {
        if (_disposed) throw new ObjectDisposedException(ToString());
        if (!Initialized) Initialize();

        Engine.Views.Add(View);
    }

    /// <inheritdoc/>
    public virtual void HookOut() => Engine.Views.Remove(View);

    /// <summary>
    /// The file format (file ending without a dot) used to store skybox textures.
    /// </summary>
    protected string SkyboxFileFormat => "jpg";

    private void UpdateSkybox()
    {
        // Clean up the old skybox if any
        if (Scene.Skybox != null)
        {
            Scene.Skybox.Dispose();
            Scene.Skybox = null;
        }

        // Allow for no skybox at all
        if (string.IsNullOrEmpty(Universe.Skybox)) return;

        // Right, Left, Up, Down, Front, Back texture filenames
        string rt = $"Skybox/{Universe.Skybox}/rt.{SkyboxFileFormat}";
        string lf = $"Skybox/{Universe.Skybox}/lf.{SkyboxFileFormat}";
        string up = $"Skybox/{Universe.Skybox}/up.{SkyboxFileFormat}";
        string dn = $"Skybox/{Universe.Skybox}/dn.{SkyboxFileFormat}";
        string ft = $"Skybox/{Universe.Skybox}/ft.{SkyboxFileFormat}";
        string bk = $"Skybox/{Universe.Skybox}/bk.{SkyboxFileFormat}";

        if (ContentManager.FileExists("Textures", up) && ContentManager.FileExists("Textures", dn))
        { // Full skybox
            Scene.Skybox = SimpleSkybox.FromAssets(Engine, rt, lf, up, dn, ft, bk);
        }
        else
        { // Cardboard-style skybox (missing top and bottom)
            Scene.Skybox = SimpleSkybox.FromAssets(Engine, rt, lf, null, null, ft, bk);
        }
    }

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

            Universe.SkyboxChanged -= UpdateSkybox;
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

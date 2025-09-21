using System;
using System.IO;
using AlphaFramework.World;
using OmegaEngine.Assets;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Renderables;

namespace AlphaFramework.Presentation;

/// <summary>
/// Maintains a <see cref="OmegaEngine.Graphics.View"/> to present an <see cref="IUniverse"/> game world.
/// </summary>
/// <typeparam name="TUniverse">The type of universe to present.</typeparam>
public interface IPresenter<out TUniverse> : IDisposable
    where TUniverse : class, IUniverse
{
    /// <summary>
    /// The engine view used to display the <see cref="Scene"/>
    /// </summary>
    View View { get; }

    /// <summary>
    /// The game world to present.
    /// </summary>
    TUniverse Universe { get; }

    /// <summary>
    /// Generate <see cref="Renderable"/>s from the <see cref="PresenterBase{TUniverse}.Universe"/> and keeps everything in sync using events
    /// </summary>
    /// <exception cref="FileNotFoundException">A required <see cref="Asset"/> file could not be found.</exception>
    /// <exception cref="IOException">There was an error reading an <see cref="Asset"/> file.</exception>
    /// <exception cref="InvalidDataException">An <see cref="Asset"/> file contains invalid data.</exception>
    /// <remarks>Should be called before <see cref="PresenterBase{TUniverse}.HookIn"/> is used</remarks>
    void Initialize();

    /// <summary>
    /// Hooks the <see cref="PresenterBase{TUniverse}.View"/> into <see cref="OmegaEngine.Engine.Views"/>
    /// </summary>
    /// <remarks>Will internally call <see cref="PresenterBase{TUniverse}.Initialize"/> first, if you didn't</remarks>
    void HookIn();

    /// <summary>
    /// Hooks the <see cref="PresenterBase{TUniverse}.View"/> out of <see cref="OmegaEngine.Engine.Views"/>
    /// </summary>
    void HookOut();
}

using AlphaFramework.Presentation;
using OmegaEngine;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using Template.AlphaFramework.World;

namespace Template.AlphaFramework.Presentation;

/// <summary>
/// Handles the visual representation of <see cref="World"/> content in the <see cref="OmegaEngine"/>
/// </summary>
public class Presenter : PresenterBase<Universe>
{
    /// <summary>
    /// Creates a new presenter.
    /// </summary>
    /// <param name="engine">The engine to use for rendering.</param>
    /// <param name="universe">The game world to present.</param>
    /// <param name="camera">The camera to use for viewing the scene.</param>
    public Presenter(Engine engine, Universe universe, Camera camera)
        : base(engine, universe)
    {
        View = new View(Scene, camera);
    }
}

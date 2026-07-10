using System.Drawing;
using OmegaEngine;
using OmegaEngine.Graphics.Cameras;
using Template.AlphaFramework.Presentation.Config;
using Template.AlphaFramework.World;

namespace Template.AlphaFramework.Presentation;

/// <summary>
/// Presents a <see cref="Universe"/> for interactive gameplay.
/// </summary>
/// <remarks>The camera reacts to input because the owning game state adds this presenter as an input receiver.</remarks>
public sealed class InGamePresenter : Presenter
{
    /// <summary>
    /// Creates a new in-game presenter.
    /// </summary>
    /// <param name="engine">The engine to use for rendering.</param>
    /// <param name="universe">The universe to display.</param>
    public InGamePresenter(Engine engine, Universe universe) : base(engine, universe)
    {
        var camera = new ArcballCamera
        {
            Name = "InGame",
            FieldOfView = Settings.Current.Graphics.FieldOfView,
            MinRadius = 2,
            MaxRadius = 200,
            Radius = 30,
            Pitch = 30
        };

        View = new(Scene, camera)
        {
            Name = "InGame",
            Lighting = true,
            BackgroundColor = Color.Black
        };
    }
}

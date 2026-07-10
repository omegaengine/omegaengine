using System.Drawing;
using OmegaEngine;
using OmegaEngine.Graphics.Cameras;
using Template.AlphaFramework.Presentation.Config;
using Template.AlphaFramework.World;

namespace Template.AlphaFramework.Presentation;

/// <summary>
/// Presents a <see cref="Universe"/> for editing in the map editor.
/// </summary>
public sealed class EditorPresenter : Presenter
{
    /// <summary>
    /// Creates a new editor presenter.
    /// </summary>
    /// <param name="engine">The engine to use for rendering.</param>
    /// <param name="universe">The universe to display.</param>
    public EditorPresenter(Engine engine, Universe universe) : base(engine, universe)
    {
        var camera = new ArcballCamera
        {
            Name = "Editor",
            FieldOfView = Settings.Current.Graphics.FieldOfView,
            MinRadius = 2,
            MaxRadius = 500,
            Radius = 50,
            Pitch = 30
        };

        View = new(Scene, camera)
        {
            Name = "Editor",
            Lighting = false,
            BackgroundColor = Color.CornflowerBlue
        };
    }
}

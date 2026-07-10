using System.Diagnostics;
using System.Drawing;
using OmegaEngine;
using OmegaEngine.Graphics.Cameras;
using Template.AlphaFramework.Presentation.Config;
using Template.AlphaFramework.World;

namespace Template.AlphaFramework.Presentation;

/// <summary>
/// Displays a slowly rotating view of a <see cref="Universe"/> as a backdrop for the main menu.
/// </summary>
public sealed class MenuPresenter : Presenter
{
    private readonly ArcballCamera _camera;

    /// <summary>
    /// Creates a new menu background presenter.
    /// </summary>
    /// <param name="engine">The engine to use for rendering.</param>
    /// <param name="universe">The universe to display.</param>
    public MenuPresenter(Engine engine, Universe universe) : base(engine, universe)
    {
        _camera = new()
        {
            Name = "Menu",
            FieldOfView = Settings.Current.Graphics.FieldOfView,
            MinRadius = 5,
            MaxRadius = 100,
            Radius = 20,
            Pitch = 20
        };

        View = new(Scene, _camera)
        {
            Name = "Menu",
            Lighting = true,
            BackgroundColor = Color.CornflowerBlue
        };
    }

    /// <inheritdoc/>
    public override void HookIn()
    {
        base.HookIn();
        View.PreRender += RotateCamera;
    }

    /// <inheritdoc/>
    public override void HookOut()
    {
        View.PreRender -= RotateCamera;
        base.HookOut();
    }

    private readonly Stopwatch _timer = Stopwatch.StartNew();

    private void RotateCamera()
    {
        _camera.Yaw += _timer.Elapsed.TotalSeconds * -5;
        _timer.Restart();
    }
}

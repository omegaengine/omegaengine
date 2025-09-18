using AlphaFramework.Presentation;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using Template.AlphaFramework.Presentation.Config;
using Template.AlphaFramework.World;

namespace Template.AlphaFramework;

public class Game(Settings settings)
    : GameBase(settings, Constants.AppName)
{
	private readonly TrackCamera _camera = new() {VerticalRotation = 20};

    /// <inheritdoc/>
    protected override bool Initialize()
    {
        if (!base.Initialize()) return false;

        // Settings update hooks
        settings.General.Changed += Program.UpdateLocale;

        UpdateStatus("Loading graphics");
        var scene = new Scene
        {
            Positionables = {Model.Sphere(Engine, XTexture.Get(Engine, "flag.png"), slices: 50, stacks: 50)}
        };
        var view = new View(scene, _camera) {BackgroundColor = Color.CornflowerBlue};
        Engine.Views.Add(view);
        Engine.FadeIn();

        LoadDialog("MainMenu");

        return true;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                // Remove settings update hooks
                settings.General.Changed -= Program.UpdateLocale;
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

    /// <inheritdoc/>
    protected override double GetElapsedGameTime(double elapsedTime)
    {
    	_camera.HorizontalRotation += elapsedTime * 100;
    	return elapsedTime;
    }
}

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
    /// <inheritdoc/>
    protected override bool Initialize()
    {
        if (!base.Initialize()) return false;

        UpdateStatus("Loading graphics");
        var scene = new Scene
        {
            Positionables = { Model.Sphere(Engine, XTexture.Get(Engine, "flag.png"), slices: 50, stacks: 50) }
        };
        var camera = new ArcballCamera { Pitch = 20 };
        var view = new View(scene, camera)
        {
            BackgroundColor = Color.CornflowerBlue
        };

        Engine.Views.Add(view);
        this.AddInputReceiver(camera);

        Engine.FadeIn();
        LoadDialog("MainMenu");

        return true;
    }
}

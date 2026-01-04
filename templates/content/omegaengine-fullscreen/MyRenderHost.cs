using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using OmegaGUI;
using OmegaGUI.Model;

namespace Template.Fullscreen;

public class MyRenderHost : RenderHost
{
    private GuiManager? _guiManager;

    public MyRenderHost() : base("Template.Fullscreen")
    {
        ToFullscreen(); // Fake fullscreen while loading
    }

    protected override bool Initialize()
    {
        if (!base.Initialize()) return false;

        InitializeGui();
        InitializeScene();
        Engine.Config = BuildEngineConfig(fullscreen: true); // Real fullscreen
        Engine.FadeIn();

        return true;
    }

    private void InitializeScene()
    {
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
    }

    private void InitializeGui()
    {
        _guiManager = new GuiManager(Engine);
        Form.WindowMessage += _guiManager.OnMsgProc;

        var dialog = new Dialog
        {
            Controls = {new Button {Text = "Exit", Location = new Point(10, 10), OnClick = "Game:Exit()"}}
        };

        var presenter = new DialogPresenter(_guiManager, dialog, lua: NewLua());
        presenter.Show();
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                _guiManager?.Dispose();
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
}


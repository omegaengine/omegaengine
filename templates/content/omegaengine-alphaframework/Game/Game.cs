using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using OmegaGUI;
using OmegaGUI.Model;

namespace Template.AlphaFramework;

class Game : GameBase
{
    private TrackCamera? _camera;
    private GuiManager? _guiManager;

    public Game() : base("Template.AlphaFramework")
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
            Positionables = {Model.Sphere(Engine, XTexture.Get(Engine, "flag.png"), slices: 50, stacks: 50)}
        };
        _camera = new TrackCamera {VerticalRotation = 20};
        var view = new View(scene, _camera) {BackgroundColor = Color.CornflowerBlue};
        Engine.Views.Add(view);
    }

    private void InitializeGui()
    {
        _guiManager = new GuiManager(Engine);
        Form.WindowMessage += _guiManager.OnMsgProc;

        var dialog = Dialog.FromContent("MainMenu.xml");
        var dialogRenderer = new DialogRenderer(_guiManager, dialog, lua: NewLua());
        dialogRenderer.Show();
    }

    protected override void Render(double elapsedTime)
    {
        if (_camera != null) _camera.HorizontalRotation += elapsedTime * 100;
        base.Render(elapsedTime);
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
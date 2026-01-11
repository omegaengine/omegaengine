using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Input;
using View = OmegaEngine.Graphics.View;

namespace Template.WinForms;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        Engine engine = renderPanel.Setup();
        InitializeScene(engine);
    }

    private void InitializeScene(Engine engine)
    {
        var scene = new Scene
        {
            Positionables = { Model.Sphere(engine, XTexture.Get(engine, "flag.png")) }
        };
        var camera = new ArcballCamera
        {
            MinRadius = 11,
            MaxRadius = 200,
            Radius = 25,
            Pitch = 30
        };
        var view = new View(scene, camera)
        {
            BackgroundColor = Color.CornflowerBlue
        };

        engine.Views.Add(view);
        renderPanel.AddInputReceiver(camera);
        renderPanel.AddInputReceiver(new ActionReceiver(engine.Render)); // Draw new frame after input is processed
    }

    private void buttonExit_Click(object sender, EventArgs e)
    {
        Close();
    }
}

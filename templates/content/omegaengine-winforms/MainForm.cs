using System;
using System.Drawing;
using System.Windows.Forms;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using View = OmegaEngine.Graphics.View;

namespace Template.WinForms
{
    public partial class MainForm : Form
    {
        private TrackCamera _camera;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Engine engine = renderPanel.Setup();
            InitializeScene(engine);
            timerRender.Enabled = true;
            engine.FadeIn();
        }

        private void InitializeScene(Engine engine)
        {
            var scene = new Scene
            {
                Positionables = { Model.Sphere(engine, XTexture.Get(engine, "flag.png")) }
            };
            _camera = new TrackCamera {VerticalRotation = 20};
            var view = new View(scene, _camera) {BackgroundColor = Color.CornflowerBlue};
            engine.Views.Add(view);
        }

        private void timerRender_Tick(object sender, EventArgs e)
        {
            _camera.HorizontalRotation += 3;
            renderPanel.Engine.Render();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

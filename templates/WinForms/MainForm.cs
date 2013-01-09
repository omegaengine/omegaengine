using System;
using System.Drawing;
using System.Windows.Forms;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using View = OmegaEngine.Graphics.View;

namespace $safeprojectname$
{
    public partial class MainForm : Form
    {
        private TrackCamera camera;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Initialize engine
            Engine engine = renderPanel.Setup();

            // Create a scene with a textured sphere
            Scene scene = new Scene(engine)
            {
                Positionables = {Model.Sphere(engine, XTexture.Get(engine, "flag.png", false), 10, 20, 20)}
            };

            // Create a camera and a blue background to view the scene
            camera = new TrackCamera(50, 50) {VerticalRotation = 20};
            engine.Views.Add(new View(engine, scene, camera) {BackgroundColor = Color.CornflowerBlue});

            // Start rendering at roughly 30 FPS
            timerRender.Enabled = true;
            engine.FadeIn();
        }

        private void timerRender_Tick(object sender, EventArgs e)
        {
            // Rotate camera
            camera.HorizontalRotation += 3;

            renderPanel.Engine.Render();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
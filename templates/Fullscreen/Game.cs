using System;
using System.Drawing;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using OmegaGUI;
using OmegaGUI.Model;

namespace Fullscreen
{
    class Game : GameBase
    {
        private TrackCamera _camera;
        private GuiManager _guiManager;

        // TODO: Add icon and background image
        public Game() : base("$projectname$", null, null, false)
        {
            // Fake fullscreen while loading
            ToFullscreen();
        }

        protected override bool Initialize()
        {
            // Initialize engine
            if (!base.Initialize()) return false;
            InitializeGui();

            // Create a scene with a textured sphere
            var scene = new Scene(Engine)
            {
                Positionables = {Model.Sphere(Engine, XTexture.Get(Engine, "flag.png", false), 10, 60, 60)}
            };

            // Create a camera and a blue background to view the scene
            _camera = new TrackCamera(50, 50) {VerticalRotation = 20};
            Engine.Views.Add(new View(Engine, scene, _camera) {BackgroundColor = Color.CornflowerBlue});

            // Switch to fullscreen mode and start rendering
            Engine.EngineConfig = BuildEngineConfig(true);
            Engine.FadeIn();
            return true;
        }

        private void InitializeGui()
        {
            // Initialize GUI subsystem
            _guiManager = new GuiManager(Engine);
            Form.WindowMessage += _guiManager.OnMsgProc;
            
            // Create a dialog with an exit button
            var dialog = new Dialog
            {
                Controls = {new Button {Text = "Exit", Location = new Point(10, 10), OnClick = "Game:Exit()"}}
            };

            // Render the dialog
            var dialogRenderer = new DialogRenderer(_guiManager, dialog, new Point(), true);
            SetupLua(dialogRenderer.Lua); // Hook up scripting objects
            dialogRenderer.Show();
        }

        protected override void Render(double elapsedTime)
        {
            // Rotate camera (FPS independent)
            _camera.HorizontalRotation += elapsedTime * 100;

            base.Render(elapsedTime);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_guiManager != null) _guiManager.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}

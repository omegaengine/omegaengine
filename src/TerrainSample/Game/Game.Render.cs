/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Windows.Forms;
using Common;
using OmegaEngine;
using OmegaGUI;
using TerrainSample.Properties;
using TerrainSample.World.Config;
using TerrainSample.World.Templates;

namespace TerrainSample
{
    partial class Game
    {
        #region Reset engine
        /// <inheritdoc/>
        protected override void ResetEngine()
        {
            if (Settings.Current.Display.Fullscreen)
            { // Fullscreen
                ToFullscreen();
            }
            else
            { // Windowed
                ToWindowed(Settings.Current.Display.WindowSize);

                // Validate window size before continuing
                if (Form.ClientSize != Settings.Current.Display.WindowSize)
                {
                    Settings.Current.Display.WindowSize = Form.ClientSize;
                    return;
                }
            }

            base.ResetEngine();
        }
        #endregion

        #region Engine configuration
        /// <inheritdoc/>
        protected override EngineConfig BuildEngineConfig(bool fullscreen)
        {
            // Note: Doesn't call base methods

            #region Safety checks
            if (!EngineCapabilities.CheckResolution(0, Settings.Current.Display.Resolution.Width, Settings.Current.Display.Resolution.Height))
                Settings.Current.Display.Resolution = Screen.PrimaryScreen.Bounds.Size;
            if (!EngineCapabilities.CheckAA(0, Settings.Current.Display.AntiAliasing))
                Settings.Current.Display.AntiAliasing = 0;
            #endregion

            var engineConfig = new EngineConfig
            {
                Fullscreen = fullscreen,
                VSync = Settings.Current.Display.VSync,
                TargetSize = fullscreen ? Settings.Current.Display.Resolution : Form.ClientSize,
                AntiAliasing = Settings.Current.Display.AntiAliasing
            };
            if (!string.IsNullOrEmpty(Settings.Current.Graphics.ForceShaderModel))
                engineConfig.ForceShaderModel = new Version(Settings.Current.Graphics.ForceShaderModel);

            return engineConfig;
        }
        #endregion

        #region Apply graphics settings
        /// <inheritdoc/>
        protected override void ApplyGraphicsSettings()
        {
            #region Apply settings
            Engine.Anisotropic = Settings.Current.Graphics.Anisotropic;
            Engine.Effects.NormalMapping = Settings.Current.Graphics.NormalMapping;
            Engine.Effects.PostScreenEffects = Settings.Current.Graphics.PostScreenEffects;
            Engine.Effects.Shadows = Settings.Current.Graphics.Shadows;
            Engine.Effects.DoubleSampling = Settings.Current.Graphics.DoubleSampling;
            Engine.Effects.WaterEffects = Settings.Current.Graphics.WaterEffects;
            Engine.Effects.ParticleSystemQuality = Settings.Current.Graphics.ParticleSystemQuality;
            #endregion

            #region Read settings back to repair any invalid stuff
            Settings.Current.Graphics.Anisotropic = Engine.Anisotropic;
            Settings.Current.Graphics.NormalMapping = Engine.Effects.NormalMapping;
            Settings.Current.Graphics.PostScreenEffects = Engine.Effects.PostScreenEffects;
            Settings.Current.Graphics.Shadows = Engine.Effects.Shadows;
            Settings.Current.Graphics.DoubleSampling = Engine.Effects.DoubleSampling;
            Settings.Current.Graphics.WaterEffects = Engine.Effects.WaterEffects;
            #endregion
        }
        #endregion

        //--------------------//

        #region Initialize
        /// <inheritdoc/>
        protected override bool Initialize()
        {
            // Run the predefined init-steps first
            if (!base.Initialize()) return false;

            // Settings update hooks
            Settings.Current.General.Changed += Program.UpdateLocale;
            Settings.Current.Controls.Changed += ApplyControlsSettings;
            Settings.Current.Display.Changed += ResetEngine;
            Settings.Current.Graphics.Changed += ApplyGraphicsSettings;
            Settings.Current.Sound.Changed += ApplySoundSettings;

            UpdateStatus(Resources.LoadingGraphics);
            Form.ResizeEnd += delegate
            {
                if (!Settings.Current.Display.Fullscreen)
                    Settings.Current.Display.WindowSize = Form.ClientSize;
            };

            using (new TimedLogEvent("Initialize GUI"))
            {
                // Initialize GUI subsystem
                GuiManager = new GuiManager(Engine);
                Form.WindowMessage += GuiManager.OnMsgProc;
            }

            using (new TimedLogEvent("Load graphics"))
            {
                EntityTemplate.LoadAll();
                ItemTemplate.LoadAll();
                TerrainTemplate.LoadAll();

                // Handle command-line arguments
                if (Program.Args.Contains("map") && !string.IsNullOrEmpty(Program.Args["map"]))
                { // Load command-line map
                    LoadMap(Program.Args["map"]);
                }
                else if (Program.Args.Contains("modify") && !string.IsNullOrEmpty(Program.Args["modify"]))
                { // Load command-line map for modification
                    ModifyMap(Program.Args["modify"]);
                }
                else if (Program.Args.Contains("benchmark"))
                { // Run automatic benchmark
                    StartBenchmark();
                }
                else
                { // Load main menu
                    PreloadPreviousSession();
                    LoadMenu(Program.Args.Contains("menu") ? Program.Args["menu"] : GetRandomMenuMap());
                }
            }

            return true;
        }
        #endregion

        #region Render
        /// <inheritdoc/>
        protected override void Render(double elapsedTime)
        {
            // Note: Doesn't call base methods

            // Check if we are currently in fake fullscreen mode (just a big window)
            if (Fullscreen && !Engine.Config.Fullscreen)
            {
                // Now switch the Direct3D device to real fullscreen mode
                Log.Info("Switch to real fullscreen mode");
                ResetEngine();
            }

            switch (CurrentState)
            {
                case GameState.InGame:
                case GameState.Modify:
                {
                    // Time passes as defined by the session
                    double elapsedGameTime = elapsedTime * CurrentSession.Universe.TimeWarpFactor;
                    CurrentSession.Universe.Update(elapsedGameTime);
                    Engine.Render(elapsedGameTime);
                    break;
                }

                case GameState.Pause:
                {
                    // Time passes very slowly and does not affect session
                    Engine.Render(elapsedTime / 10);
                    break;
                }

                default:
                {
                    // Time passes normally but there is no session
                    Engine.Render();
                    break;
                }
            }

            // Start new song after last one has stopped
            if (Settings.Current.Sound.PlayMusic) Engine.Music.Update();
        }
        #endregion
    }
}

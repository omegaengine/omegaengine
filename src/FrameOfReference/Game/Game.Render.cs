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

using System.Windows.Forms;
using FrameOfReference.Presentation.Config;
using FrameOfReference.Properties;
using FrameOfReference.World.Templates;
using NanoByte.Common;
using OmegaEngine;

namespace FrameOfReference;

partial class Game
{
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

    /// <inheritdoc/>
    protected override EngineConfig BuildEngineConfig(bool fullscreen)
    {
        // Note: Doesn't call base methods

        if (!EngineCapabilities.CheckResolution(0, Settings.Current.Display.Resolution.Width, Settings.Current.Display.Resolution.Height))
            Settings.Current.Display.Resolution = Screen.PrimaryScreen.Bounds.Size;
        if (!EngineCapabilities.CheckAA(0, Settings.Current.Display.AntiAliasing))
            Settings.Current.Display.AntiAliasing = 0;

        var engineConfig = new EngineConfig
        {
            Fullscreen = fullscreen,
            VSync = Settings.Current.Display.VSync,
            TargetSize = fullscreen ? Settings.Current.Display.Resolution : Form.ClientSize,
            AntiAliasing = Settings.Current.Display.AntiAliasing
        };
        if (Settings.Current.Display.ForceShaderModel is {} forceShaderModel)
            engineConfig.ForceShaderModel = new(forceShaderModel);

        return engineConfig;
    }

    /// <inheritdoc/>
    protected override void ApplyGraphicsSettings()
    {
        Engine.Anisotropic = Settings.Current.Graphics.Anisotropic;
        Engine.Effects.NormalMapping = Settings.Current.Graphics.NormalMapping;
        Engine.Effects.PostScreenEffects = Settings.Current.Graphics.PostScreenEffects;
        Engine.Effects.DoubleSampling = Settings.Current.Graphics.DoubleSampling;
        Engine.Effects.WaterEffects = Settings.Current.Graphics.WaterEffects;

        Settings.Current.Graphics.Anisotropic = Engine.Anisotropic;
        Settings.Current.Graphics.NormalMapping = Engine.Effects.NormalMapping;
        Settings.Current.Graphics.PostScreenEffects = Engine.Effects.PostScreenEffects;
        Settings.Current.Graphics.DoubleSampling = Engine.Effects.DoubleSampling;
        Settings.Current.Graphics.WaterEffects = Engine.Effects.WaterEffects;
    }

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

        UpdateStatus(Resources.LoadingGraphics);
        Form.ResizeEnd += delegate
        {
            if (!Settings.Current.Display.Fullscreen)
                Settings.Current.Display.WindowSize = Form.ClientSize;
        };

        using (new TimedLogEvent("Initialize GUI"))
        {
            // Initialize GUI subsystem
            GuiManager = new(Engine);
            Form.WindowMessage += GuiManager.OnMsgProc;
        }

        using (new TimedLogEvent("Load graphics"))
        {
            EntityTemplate.LoadAll();
            TerrainTemplate.LoadAll();

            // Handle command-line arguments
            if (Program.Args["map"] is {} map)
            { // Load command-line map
                LoadMap(map);
            }
            else if  (Program.Args["modify"] is {} modify)
            { // Load command-line map for modification
                ModifyMap(modify);
            }
            else if (Program.Args.Contains("benchmark"))
            { // Run automatic benchmark
                StartBenchmark();
            }
            else
            { // Load main menu
                PreloadPreviousSession();
                LoadMenu(Program.Args["menu"] ?? GetMenuMap());
            }
        }

        return true;
    }

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
                Engine.Render(CurrentSession.Update(elapsedTime));
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
    }
}

/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows.Forms;
using LuaInterface;
using NanoByte.Common;
using NanoByte.Common.Storage.SlimDX;
using OmegaEngine.Graphics.Shaders;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine
{
    partial class GameBase
    {
        #region Reset engine
        /// <summary>
        /// Resets the <see cref="Engine"/>
        /// </summary>
        [LuaGlobal(Description = "Resets the graphics engine")]
        protected virtual void ResetEngine()
        {
            Engine.Config = BuildEngineConfig(Fullscreen);
            Engine.Render();
        }
        #endregion

        #region Engine configuration
        /// <summary>
        /// Called to generate an <see cref="EngineConfig"/> based on external settings
        /// </summary>
        /// <param name="fullscreen">Shall the configuration be generated for fullscreen mode?</param>
        protected virtual EngineConfig BuildEngineConfig(bool fullscreen)
        {
            return new EngineConfig
            {
                Fullscreen = fullscreen,
                TargetSize = fullscreen ? Screen.PrimaryScreen.Bounds.Size : Form.ClientSize
            };
        }
        #endregion

        #region Apply graphics settings
        /// <summary>
        /// Called when graphics settings from an external source need to be applied to the <see cref="Engine"/>
        /// </summary>
        protected virtual void ApplyGraphicsSettings()
        {}
        #endregion

        //--------------------//

        #region Initialize
        /// <summary>
        /// To be called after the window is ready and the <see cref="Engine"/> needs to be set up
        /// </summary>
        /// <returns><see langword="true"/> if the initialization worked, <see langword="false"/> if it failed an the app must be closed</returns>
        protected virtual bool Initialize()
        {
            using (new TimedLogEvent("Initialize engine"))
            {
                // Initialize engine
                try
                {
                    Engine = new Engine(Form, BuildEngineConfig(fullscreen: false)); // No fullscreen while loading
                    ApplyGraphicsSettings();
                }
                    #region Error handling
                catch (NotSupportedException ex)
                {
                    Log.Error(ex + "\n" + ex.InnerException);
                    Msg.Inform(Form, Resources.BadGraphics, MsgSeverity.Error);
                    Exit();
                    return false;
                }
                catch (Direct3D9NotFoundException)
                {
                    Msg.Inform(Form, Resources.DirectXMissing, MsgSeverity.Error);
                    Exit();
                    return false;
                }
                catch (Direct3DX9NotFoundException)
                {
                    Msg.Inform(Form, Resources.DirectXMissing, MsgSeverity.Error);
                    Exit();
                    return false;
                }
                catch (Direct3D9Exception ex)
                {
                    Msg.Inform(Form, ex.Message, MsgSeverity.Error);
                    Exit();
                    return false;
                }
                catch (SlimDX.DirectSound.DirectSoundException ex)
                {
                    Msg.Inform(Form, ex.Message, MsgSeverity.Error);
                    Exit();
                    return false;
                }
                #endregion

                if (Engine.Capabilities.MaxShaderModel < TerrainShader.MinShaderModel)
                {
                    Log.Error("No support for Pixel Shader " + TerrainShader.MinShaderModel);
                    Msg.Inform(Form, Resources.BadGraphics + "\n" +
                                     string.Format(Resources.MinimumShaderModel, TerrainShader.MinShaderModel),
                        MsgSeverity.Warn);
                    Exit();
                    return false;
                }
            }

            // Only load music subsystem if there is a library
            if (ContentManager.FileExists("Music", "list.txt"))
            {
                using (new TimedLogEvent("Load music"))
                {
                    UpdateStatus(Resources.LoadingMusic);
                    Engine.Music.LoadLibrary("list.txt");
                }
            }

            return true;
        }
        #endregion

        #region Render
        /// <summary>
        /// Called when the next frame needs to be rendered.
        /// </summary>
        /// <param name="elapsedTime">The number of seconds that have passed since this method was last called.</param>
        protected virtual void Render(double elapsedTime)
        {
            // Use the form's WindowState to determine whether fullscreen mode was intended
            if (Form.WindowState == FormWindowState.Maximized)
            {
                // Switch the form's WindowState to normal, otherwise there will be ugly render bugs
                Form.WindowState = FormWindowState.Normal;

                // Now switch the Direct3D device to real fullscreen mode
                Log.Info("Switch to real fullscreen mode");
                ResetEngine();
            }

            Engine.Render(elapsedTime);

            // Start new song after last one has stopped
            Engine.Music.Update();
        }
        #endregion
    }
}

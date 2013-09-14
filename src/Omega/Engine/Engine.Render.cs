/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Common;
using LuaInterface;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Shaders;
using SlimDX;
using SlimDX.Direct3D9;
using WinForms = System.Windows.Forms;

namespace OmegaEngine
{
    // This file contains the code for the actual render loop
    partial class Engine
    {
        #region Events
        /// <summary>
        /// Occurs once for every frame before rendering.
        /// </summary>
        [Description("Occurs once for every frame before rendering.")]
        public event Action PreRender;

        /// <summary>
        /// Allows the integration of external render processes at the end of the <see cref="Engine"/>'s presentation scheme.
        /// </summary>
        [Description("Allows the integration of external render processes at the end of the Engine's presentation scheme.")]
        public event Action ExtraRender;

        /// <summary>
        /// Occurs once for every frame after rendering
        /// </summary>
        [Description("Occurs once for every frame after rendering.")]
        public event Action PostRender;

        /// <summary>
        /// Occurs after the <see cref="Device"/> was lost. This usually happens when switching to or from fullscreen mode.
        /// </summary>
        [Description("Occurs after the DirectX Device was lost. This usually happens when switching to or from fullscreen mode.")]
        public event Action DeviceLost;

        /// <summary>
        /// Occurs after the <see cref="Device"/> was reset. This needs to be done to continue using it after <see cref="DeviceLost"/>.
        /// </summary>
        [Description("Occurs after the DirectX Device was reset. This needs to be done to continue using it after DeviceLost.")]
        public event Action DeviceReset;
        #endregion

        #region Variables
        private bool _firstFrame = true;
        private bool _isResetting;
        internal readonly Mesh SimpleSphere;
        internal readonly Mesh SimpleBox;
        #endregion

        #region Properties
        // Order is important, duplicate entries are not allowed
        private readonly C5.IList<View> _views = new C5.HashedArrayList<View>();

        /// <summary>
        /// A list of all views to be rendered by the engine
        /// </summary>
        public IList<View> Views { get { return _views; } }

        #region DirectX
        /// <summary>
        /// The Direct3D device
        /// </summary>
        [LuaHide]
        public Device Device { get; private set; }

        /// <summary>
        /// The engine will be reset on the next <see cref="Render()" /> call
        /// </summary>
        /// <remarks>
        ///   <para>Do not try to add new assets while this is true!</para>
        ///   <para>Just run <see cref="Render()" /> to fix the problem.</para>
        /// </remarks>
        public bool NeedsReset { get; private set; }

        #region Helper
        /// <summary>
        /// Generates a set of <see cref="SlimDX.Direct3D9.PresentParameters"/> for the engine
        /// </summary>
        /// <param name="engineConfig">The <see cref="EngineConfig"/> with settings to initialize the engine</param>
        /// <returns>The generated set of <see cref="SlimDX.Direct3D9.PresentParameters"/>.</returns>
        private static PresentParameters BuildPresentParams(EngineConfig engineConfig)
        {
            var presentParams = new PresentParameters
            {
                Windowed = (!engineConfig.Fullscreen),
                SwapEffect = SwapEffect.Discard,
                PresentationInterval = (engineConfig.VSync ? PresentInterval.One : PresentInterval.Immediate),
                Multisample = (MultisampleType)engineConfig.AntiAliasing,
                BackBufferWidth = engineConfig.TargetSize.Width,
                BackBufferHeight = engineConfig.TargetSize.Height,
                BackBufferFormat = Format.X8R8G8B8, // Format.R5G6B5 for 16bit
                EnableAutoDepthStencil = true
            };

            // Automatically use best-possible ZBuffer
            if (TestDepthStencil(engineConfig.Adapter, Format.D32))
                presentParams.AutoDepthStencilFormat = Format.D32;
            else if (TestDepthStencil(engineConfig.Adapter, Format.D24X8))
                presentParams.AutoDepthStencilFormat = Format.D24X8;
            else
                presentParams.AutoDepthStencilFormat = Format.D16;

            return presentParams;
        }

        /// <summary>
        /// Checks whether a certain depth stencil format is supported by the <see cref="SlimDX.Direct3D9.Device"/>
        /// </summary>
        private static bool TestDepthStencil(int adapter, Format depthFormat)
        {
            using (var manager = new Direct3D())
            {
                return manager.CheckDeviceFormat(adapter, DeviceType.Hardware, Format.X8R8G8B8,
                    Usage.DepthStencil, ResourceType.Surface, depthFormat);
            }
        }
        #endregion

        /// <summary>
        /// The <see cref="PresentParameters"/> auto-generated from <see cref="EngineConfig"/>
        /// </summary>
        internal PresentParameters PresentParams { get; private set; }

        /// <summary>
        /// The BackBuffer the D3D Device renders onto
        /// </summary>
        internal Surface BackBuffer { get; private set; }

        /// <summary>
        /// The viewport for the render target
        /// </summary>
        public Viewport RenderViewport { get; private set; }

        /// <summary>
        /// The size of the render target
        /// </summary>
        public Size RenderSize
        {
            get { return new Size(RenderViewport.Width, RenderViewport.Height); }
            set
            {
                RenderViewport = new Viewport
                {
                    Width = value.Width,
                    Height = value.Height,
                    X = RenderViewport.X,
                    Y = RenderViewport.Y,
                    MinZ = RenderViewport.MinZ,
                    MaxZ = RenderViewport.MaxZ
                };
            }
        }
        #endregion

        #region Shader
        /// <summary>
        /// The base directory where shader files are stored
        /// </summary>
        public string ShaderDir { private set; get; }

        /// <summary>
        /// The maximum shader model version to be used (2.a is replaced by 2.0.1, 2.b is replaced by 2.0.2)
        /// </summary>
        public Version MaxShaderModel { get; private set; }

        /// <summary>
        /// A shader used for default lighting
        /// </summary>
        public GeneralShader DefaultShader { get; private set; }

        private WaterShader _simpleWaterShader;

        /// <summary>
        /// A shader used for simple water (no reflection or refraction)
        /// </summary>
        internal WaterShader SimpleWaterShader { get { return _simpleWaterShader = _simpleWaterShader ?? new WaterShader(this); } }
        #endregion

        #endregion

        //--------------------//

        #region Render
        /// <summary>
        /// To be called by the render loop
        /// </summary>
        /// <param name="elapsedTime">How much game time in seconds has elapsed since the last frame</param>
        /// <param name="noPresent"><see langword="true"/> to supress actually displaying the render output at the end.</param>
        public void Render(double elapsedTime, bool noPresent = false)
        {
            #region Frame counting
            ThisFrameTime = elapsedTime;
            TotalTime += elapsedTime;
            TotalFrames++;
            #endregion

            #region Sanity checks
            // Unlock the mouse and skip rendering if the target is invisible or the device disposed
            if (!Target.Visible || _isResetting || Disposed)
            {
                WinForms.Cursor.Clip = new Rectangle();
                return;
            }

            // Lock mouse cursor inside window to prevent glitches with multi-monitor systems
            if (_engineConfig.Fullscreen) WinForms.Cursor.Clip = new Rectangle(new Point(), RenderSize);

            // Detect and handle lost device (Note: Reset can also be triggered from elsewhere)
            if (Device.TestCooperativeLevel() != ResultCode.Success)
            {
                Log.Info("D3D Device lost");
                NeedsReset = true;
            }
            if (NeedsReset) Reset();
            #endregion

            // Handle pending resets
            while (_pendingReset.Count > 0) _pendingReset.Dequeue().Reset();

            // Raise PreRender event only if this is a normal render run
            if (PreRender != null && !noPresent) PreRender();

            #region Frame log
            if (_frameLogMode != FrameLog.Off)
            {
                // Setup log XML document
                Profiler.LogXml = new XmlDocument();

                // Log the GPU time as well
                if (_frameLogMode == FrameLog.CpuGpu)
                {
                    Profiler.DeviceQuery = new Query(Device, QueryType.Event);
                    Profiler.AddEvent("Forcing render pipeline stalls to log GPU performance");
                }
            }

            if (_firstFrame)
            {
                _firstFrame = false;
                Log.Info("Render first frame");
            }
            #endregion

            #region Timing
            if (_frameTimer.IsRunning)
            {
                _lastFrameTime = _frameTimer.Elapsed.TotalSeconds;
                _frameTimer.Reset();
            }
            _frameTimer.Start();

            if (_fpsTimer.IsRunning)
            {
                _fpsFrames++;
                if (_fpsFrames > FpsResetLimit)
                {
                    _fpsTimer.Stop();

                    Fps = (float)(_fpsFrames / _fpsTimer.Elapsed.TotalSeconds);
                    FrameMs = (float)(_fpsTimer.Elapsed.TotalMilliseconds / _fpsFrames);
                    _fpsFrames = 0;

                    _fpsTimer.Reset();
                }
            }
            _fpsTimer.Start();
            #endregion

            #region 3D Sound
            // ToDo: Properly sync with camera
            //Camera camera = mainView.Camera;
            //_listener.Deferred = true;
            //_listener.Position = camera.EffectivePosition;
            //_listener.Orientation = new DirectSound.Listener3DOrientation();
            //_listener.CommitDeferredSettings();
            #endregion

            if (!Disposed)
            {
                try
                {
                    #region Render
                    using (new ProfilerEvent("Render engine views"))
                    {
                        foreach (View view in _views.Where(view => view.Visible))
                            view.Render();
                    }

                    // Fade the output to black
                    Action fade = delegate
                    {
                        Device.Viewport = RenderViewport;
                        Device.BeginScene();
                        AlphaBlend = 255 - FadeLevel;
                        this.DrawQuadColored(Color.Black);
                        Device.EndScene();
                    };

                    if (FadeLevel > 0 && !FadeExtra)
                        using (new ProfilerEvent("Fullscreen fading before ExtraRender")) fade();

                    // Render extenal stuff like GUIs
                    if (ExtraRender != null)
                    {
                        using (new ProfilerEvent("ExtraRender delegate"))
                        {
                            Device.Viewport = RenderViewport;
                            Device.BeginScene();
                            AlphaBlend = 0;
                            ExtraRender();
                            Device.EndScene();
                        }
                    }

                    if (FadeLevel > 0 && FadeExtra)
                        using (new ProfilerEvent("Fullscreen fading after GUI")) fade();

                    if (!noPresent) Device.Present();
                    #endregion
                }
                    #region Error handling
                catch (Direct3D9Exception ex)
                {
                    if (ex.ResultCode == ResultCode.DeviceLost)
                    {
                        // A lost device will be handled in the next frame
                        return;
                    }

                    if (ex.ResultCode == ResultCode.InvalidData)
                        throw new InvalidDataException("Invalid data passed to a DirectX object", ex);

                    throw;
                }
                #endregion
            }

            #region Frame log
            if (_frameLogMode != FrameLog.Off)
            {
                _frameLogMode = FrameLog.Off;

                // Write to disk
                var writer = new XmlTextWriter(_frameLogFile, new UTF8Encoding(false)) {Formatting = Formatting.Indented};
                Profiler.LogXml.Save(writer);
                writer.Close();

                // Clean up
                Profiler.LogXml = null;
                if (Profiler.DeviceQuery != null)
                {
                    Profiler.DeviceQuery.Dispose();
                    Profiler.DeviceQuery = null;
                }
            }
            #endregion

            // Raise PostRender event only if this is a normal render run
            if (PostRender != null && !noPresent) PostRender();
        }

        /// <summary>
        /// To be called by the render loop
        /// </summary>
        public void Render()
        {
            Render(_lastFrameTime);
        }
        #endregion

        #region Reset
        /// <summary>
        /// Resets the engine
        /// </summary>
        private void Reset()
        {
            // Set flag to prevent infinite loop of resets
            _isResetting = true;

            #region Cleanup
            Log.Info("Clearing Direct3D resources");
            BackBuffer.Dispose();
            if (DeviceLost != null) DeviceLost();
            #endregion

            #region Wait
            // Wait in case the device isn't ready to be reset
            while (Device.TestCooperativeLevel() == ResultCode.DeviceLost)
            {
                Thread.Sleep(100);
                WinForms.Application.DoEvents();
            }

            // Catch internal driver errors
            var result = Device.TestCooperativeLevel();
            if (result != ResultCode.DeviceNotReset && result != ResultCode.Success) throw new Direct3D9Exception(result);
            #endregion

            #region Reset
            Log.Info("Resetting Direct3D device");
            Log.Info("Presentation parameters:\n" + PresentParams);
            Device.Reset(PresentParams);

            // Setup the default values for the Direct3D device and restore resources
            SetupTextureFiltering();
            RenderViewport = Device.Viewport;
            BackBuffer = Device.GetBackBuffer(0, 0);
            if (DeviceReset != null) DeviceReset();

            // Render states got reset to their default values on the device. Update the engine variables accordingly.
            _fillMode = FillMode.Solid;
            _cullMode = Cull.Counterclockwise;
            _zBufferMode = ZBufferMode.Normal;
            _ffpLighting = true;
            _fog = false;
            _fogColor = Color.Empty;
            _fogStart = _fogEnd = 0;
            _alphaBlend = 0;
            _worldTransform = _viewTransform = _projectionTransform = new Matrix();

            // Reset the framerate timer
            _fpsTimer.Reset();
            #endregion

            _isResetting = NeedsReset = false;
        }
        #endregion
    }
}

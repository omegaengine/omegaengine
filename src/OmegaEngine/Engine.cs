/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using LuaInterface;
using NanoByte.Common;
using NanoByte.Common.Storage;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Graphics.Shaders;
using OmegaEngine.Properties;
using SlimDX.Direct3D9;

namespace OmegaEngine;

#region Interfaces
internal interface IResetable
{
    /// <summary>
    /// Is to be called at the beginning of a frame.
    /// </summary>
    void Reset();
}
#endregion

/// <summary>
/// Provides central control for 3D rendering, sound management, asset caching, etc.
/// </summary>
public sealed partial class Engine : EngineElement
{
    #region Variables
    private readonly Direct3D _direct3D;

    /// <summary>
    /// A list of possible <see cref="View"/>s usable for rendering <see cref="Water"/>
    /// </summary>
    /// <seealso cref="WaterViewSource.FromEngine"/>
    internal readonly HashSet<WaterViewSource> WaterViewSources = [];
    #endregion

    #region Properties
    /// <summary>
    /// The culture used for loading the assembly resources.
    /// </summary>
    public static CultureInfo ResourceCulture { get => Resources.Culture; set => Resources.Culture = value; }

    /// <summary>
    /// The version number of the engine.
    /// </summary>
    public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

    /// <summary>
    /// The <see cref="System.Windows.Forms.Control"/> the engine draws onto.
    /// </summary>
    [LuaHide]
    public Control Target { get; }

    private EngineConfig _config;

    /// <summary>
    /// The settings the engine was initialized with.
    /// </summary>
    /// <remarks>Changing this will cause the engine to reset on the next <see cref="Render()" /> call</remarks>
    public EngineConfig Config
    {
        get => _config;
        set =>
            value.To(ref _config, delegate
            {
                // The device needs to be reset with changed presentation parameters
                if (PresentParams != null)
                {
                    Log.Info("Engine.Config modified");
                    NeedsReset = true;
                }
                PresentParams = BuildPresentParams(_config);
            });
    }

    /// <summary>
    /// Methods for determining the rendering capabilities of the graphics hardware.
    /// </summary>
    public EngineCapabilities Capabilities { get; }

    /// <summary>
    /// Turn specific rendering effects in the engine on or off.
    /// </summary>
    public EngineEffects Effects { get; private set; }

    /// <summary>
    /// Used by <see cref="Renderable"/> implementations to manipulate the graphics render state. Should not be manipulated manually.
    /// </summary>
    public EngineState State { get; }

    /// <summary>
    /// Tracks the performance/speed of the engine.
    /// </summary>
    public EnginePerformance Performance { get; }

    /// <summary>
    /// The central cache used for all graphics and sound assets.
    /// </summary>
    public CacheManager Cache { get; } = new();
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes the Engine and its components.
    /// </summary>
    /// <param name="target">The <see cref="System.Windows.Forms.Control"/> the engine should draw onto.</param>
    /// <param name="config">Settings for initializing the engine.</param>
    /// <exception cref="NotSupportedException">The graphics card does not meet the engine's minimum requirements.</exception>
    /// <exception cref="Direct3D9NotFoundException">Throw if required DirectX version is missing.</exception>
    /// <exception cref="Direct3DX9NotFoundException">Throw if required DirectX version is missing.</exception>
    /// <exception cref="Direct3D9Exception">Internal errors occurred while initializing the graphics card.</exception>
    /// <exception cref="SlimDX.DirectSound.DirectSoundException">Internal errors occurred while initializing the sound card.</exception>
    public Engine(Control target, EngineConfig config)
    {
        #region Sanity checks
        if (target == null) throw new ArgumentNullException(nameof(target));
        #endregion

        Engine = this;
        RegisterChild(_views);

        _direct3D = new();
        Target = target;
        Config = config;
        ShaderDir = Path.Combine(Locations.InstallBase, "Shaders");

        Capabilities = new(_direct3D, config);
        Effects = new(Capabilities) {PerPixelLighting = true};

        try
        {
            CreateDevice();
            State = new(Device);
            SetupTextureFiltering();
            Performance = new(Device, RenderPure);

            if (GeneralShader.MinShaderModel <= Capabilities.MaxShaderModel)
                RegisterChild(DefaultShader = new());

            // Create simple default meshes ready
            SimpleSphere = Mesh.CreateSphere(Device, 1, 12, 12);
            SimpleBox = Mesh.CreateBox(Device, 1, 1, 1);

            SetupAudio();
        }
        #region Error handling
        catch (Direct3D9Exception ex) when (ex.ResultCode == ResultCode.NotAvailable)
        {
            Dispose();
            throw new NotSupportedException(Resources.NotAvailable, ex);
        }
        catch (Exception)
        {
            Dispose();
            throw;
        }
        #endregion
    }

    /// <summary>
    /// Helper method for the constructor that creates the <see cref="Device"/>.
    /// </summary>
    /// <exception cref="Direct3D9Exception">Internal errors occurred during creation.</exception>
    /// <exception cref="Direct3D9NotFoundException">Throw if required DirectX version is missing.</exception>
    /// <exception cref="Direct3DX9NotFoundException">Throw if required DirectX version is missing.</exception>
    private void CreateDevice()
    {
        Device = new(_direct3D, Config.Adapter, DeviceType.Hardware, Target.Handle, GetFlags(), PresentParams);
        RenderViewport = Device.Viewport;
        BackBuffer = Device.GetBackBuffer(0, 0);

        CreateFlags GetFlags()
        {
            switch (Capabilities)
            {
                case {PureDevice: true}:
                    Log.Info("Creating Direct3D device with Hardware Vertex Processing & Pure Device");
                    return CreateFlags.FpuPreserve | CreateFlags.HardwareVertexProcessing | CreateFlags.PureDevice;

                case {HardwareVertexProcessing: true}:
                    Log.Info("Creating Direct3D device with Hardware Vertex Processing");
                    return CreateFlags.FpuPreserve | CreateFlags.HardwareVertexProcessing;

                default:
                    Log.Info("Creating Direct3D device with Software Vertex Processing");
                    return CreateFlags.FpuPreserve | CreateFlags.SoftwareVertexProcessing;
            }
        }
    }
    #endregion

    //--------------------//

    #region Reset queue
    private readonly Queue<IResetable> _pendingReset = new();

    /// <summary>
    /// Queues an object for resetting at the beginning of the next frame.
    /// </summary>
    /// <param name="o">The entity to be reset.</param>
    internal void QueueReset(IResetable o)
    {
        if (!_pendingReset.Contains(o)) _pendingReset.Enqueue(o);
    }
    #endregion

    //--------------------//

    #region Dispose
    protected override void OnDispose()
    {
        Log.Info($"Disposing engine\nLast framerate: {Performance.Fps}");

        // Dispose scenes and views
        ExtraRender = null;
        foreach (var view in Views) view.Scene.Dispose();
        base.OnDispose();

        // Shutdown music
        Music?.Dispose();

        // Dispose cached assets
        Cache?.Dispose();

        // Dispose default meshes
        SimpleSphere?.Dispose();
        SimpleBox?.Dispose();

        // Dispose Direct3D device
        BackBuffer?.Dispose();
        Device?.Dispose();

        // Dispose DirectSound objects
        //_listener?.Dispose();
        AudioDevice?.Dispose();

        _direct3D?.Dispose();

        // Dispose debug window
        _debugForm?.Dispose();
    }
    #endregion
}

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
using System.Collections.Generic;
using System.IO;
using AlphaFramework.Presentation;
using AlphaFramework.World.Positionables;
using FrameOfReference.Presentation.Config;
using FrameOfReference.World;
using NanoByte.Common.Dispatch;
using OmegaEngine;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Foundation.Storage;
using SlimDX;

namespace FrameOfReference.Presentation;

/// <summary>
/// Handles the visual representation of <see cref="World"/> content in the <see cref="OmegaEngine"/>
/// </summary>
public abstract partial class Presenter : CoordinatePresenter<Universe, Vector2>
{
    #region Properties
    private bool _wireframeTerrain;

    /// <summary>
    /// Render the <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> in wireframe-mode
    /// </summary>
    public bool WireframeTerrain
    {
        get => _wireframeTerrain;
        set
        {
            _wireframeTerrain = value;
            if (Terrain != null) Terrain.Wireframe = value;
        }
    }

    private bool _wireframeEntities;

    /// <summary>
    /// Render all entities in wireframe-mode
    /// </summary>
    public bool WireframeEntities
    {
        get => _wireframeEntities;
        set
        {
            _wireframeEntities = value;
            foreach (var positionable in RenderablesSync.Representations)
                positionable.Wireframe = value;
        }
    }

    private bool _boundingSpheresEntities;

    /// <summary>
    /// Visualize the bounding spheres of all entities
    /// </summary>
    public bool BoundingSphereEntities
    {
        get => _boundingSpheresEntities;
        set
        {
            _boundingSpheresEntities = value;
            foreach (var positionable in RenderablesSync.Representations)
                positionable.DrawBoundingSphere = value;
        }
    }

    private bool _boundingBoxEntities;

    /// <summary>
    /// Visualize the bounding boxes of all entities
    /// </summary>
    public bool BoundingBoxEntities
    {
        get => _boundingBoxEntities;
        set
        {
            _boundingBoxEntities = value;
            foreach (var positionable in RenderablesSync.Representations)
                positionable.DrawBoundingBox = value;
        }
    }
    #endregion

    /// <summary>
    /// Creates a new presenter.
    /// </summary>
    /// <param name="engine">The engine to use for rendering.</param>
    /// <param name="universe">The game world to present.</param>
    protected Presenter(Engine engine, Universe universe) : base(engine, universe)
    {}

    //--------------------//

    #region Initialize
    /// <inheritdoc/>
    public override void Initialize()
    {
        if (Initialized) return;

        if (Lighting)
        {
            SetupLighting();
            UpdateLighting();
            Universe.LightingChanged += UpdateLighting;
        }

        UpdateSkybox();
        Universe.SkyboxChanged += UpdateSkybox;

        SetupTerrain();

        base.Initialize();
    }

    /// <summary>
    /// To be called by <see cref="IDisposable.Dispose"/> and the object destructor.
    /// </summary>
    /// <param name="disposing"><c>true</c> if called manually and not by the garbage collector.</param>
    protected override void Dispose(bool disposing)
    {
        try
        {
            // Remove event handlers watching the universe
            Universe.LightingChanged -= UpdateLighting;
            Universe.SkyboxChanged -= UpdateSkybox;
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
    #endregion

    #region Terrain
    /// <summary>
    /// The <see cref="OmegaEngine"/> representation of <see cref="World.Universe.Terrain"/>
    /// </summary>
    public Terrain? Terrain { get; private set; }

    /// <summary>
    /// Helper method for setting up the <see cref="OmegaEngine.Graphics.Renderables.Terrain"/>.
    /// </summary>
    private void SetupTerrain()
    {
        // Build texture array
        var textures = new string[Universe.Terrain.Templates.Length];
        for (int i = 0; i < textures.Length; i++)
        {
            if (Universe.Terrain.Templates[i] != null && !string.IsNullOrEmpty(Universe.Terrain.Templates[i].Texture))
            {
                // Prefix directory name
                textures[i] = Path.Combine("Terrain", Universe.Terrain.Templates[i].Texture);
            }
        }

        // Create Engine Terrain and add to the Scene
        Terrain = Terrain.Create(
            Engine, Universe.Terrain.Size,
            Universe.Terrain.Size.StretchH, Universe.Terrain.Size.StretchV,
            Universe.Terrain.HeightMap ?? throw new InvalidOperationException("Terrain height map missing"),
            Universe.Terrain.TextureMap ?? throw new InvalidOperationException("Terrain texture map missing"),
            textures,
            Universe.Terrain.OcclusionIntervalMap,
            Lighting,
            Settings.Current.Graphics.TerrainBlockSize);
        Terrain.Wireframe = WireframeTerrain;
        Scene.Positionables.Add(Terrain);
    }

    /// <summary>
    /// Rebuilds the terrain from <see cref="World.Universe.Terrain"/> to reflect any modifications performed.
    /// </summary>
    public void RebuildTerrain()
    {
        if (Terrain != null)
        {
            View.Scene.Positionables.Remove(Terrain);
            Terrain.Dispose();
            Terrain = null;
        }
        SetupTerrain();
    }
    #endregion

    #region Skybox
    private void UpdateSkybox()
    {
        // Clean up the old skybox if any
        if (Scene.Skybox != null)
        {
            Scene.Skybox.Dispose();
            Scene.Skybox = null;
        }

        // Allow for no skybox at all
        if (string.IsNullOrEmpty(Universe.Skybox)) return;

        // Right, Left, Up, Down, Front, Back texture filenames
        string rt = $"Skybox/{Universe.Skybox}/rt.jpg";
        string lf = $"Skybox/{Universe.Skybox}/lf.jpg";
        string up = $"Skybox/{Universe.Skybox}/up.jpg";
        string dn = $"Skybox/{Universe.Skybox}/dn.jpg";
        string ft = $"Skybox/{Universe.Skybox}/ft.jpg";
        string bk = $"Skybox/{Universe.Skybox}/bk.jpg";

        if (ContentManager.FileExists("Textures", up) && ContentManager.FileExists("Textures", dn))
        { // Full skybox
            Scene.Skybox = SimpleSkybox.FromAssets(Engine, rt, lf, up, dn, ft, bk);
        }
        else
        { // Cardboard-style skybox (missing top and bottom)
            Scene.Skybox = SimpleSkybox.FromAssets(Engine, rt, lf, null, null, ft, bk);
        }
    }
    #endregion

    //--------------------//

    #region Camera
    /// <summary>
    /// Creates a new camera based on a state usually loaded from the <see cref="Universe"/>.
    /// </summary>
    /// <param name="state">The state to place the new camera in; <c>null</c> for default (looking at the center of the terrain).</param>
    /// <returns>The newly created <see cref="Camera"/>.</returns>
    protected Camera CreateCamera(CameraState<Vector2>? state = null)
    {
        state ??= new() {Name = "Main", Position = Universe.Terrain.Center, Radius = 1500};

        return new StrategyCamera(
            minRadius: 200, maxRadius: MaxCameraRadius,
            minAngle: 25, maxAngle: 60,
            heightController: CameraController)
        {
            Name = state.Name,
            Target = Universe.Terrain.ToEngineCoords(state.Position),
            Radius = state.Radius,
            HorizontalRotation = state.Rotation,
            FarClip = Universe.Fog ? Universe.FogDistance : 1e+6f
        };
    }

    /// <summary>
    /// The value for <see cref="StrategyCamera.MaxRadius"/>.
    /// </summary>
    protected virtual double MaxCameraRadius => 2250;

    /// <summary>
    /// Ensures the camera does not go under or outside the <see cref="Terrain"/>.
    /// </summary>
    /// <returns>The minimum height the camera must have.</returns>
    protected virtual double CameraController(DoubleVector3 coordinates)
    {
        const float floatAboveGround = 100;
        return Universe.Terrain.ToEngineCoords(coordinates.Flatten()).Y + floatAboveGround;
    }

    /// <summary>
    /// Retrieves the current state of the <see cref="Camera"/> for storage in the <see cref="Universe"/>.
    /// </summary>
    /// <returns>The current state of  the <see cref="Camera"/> or <c>null</c> if it can not be determined at this time (e.g. cinematic animation in progress).</returns>
    public CameraState<Vector2>? CameraState
    {
        get
        {
            var dispatcher = new PerTypeDispatcher<Camera, CameraState<Vector2>>()
            {
                (StrategyCamera camera) => new()
                {
                    Name = camera.Name,
                    Position = camera.Target.Flatten(),
                    Radius = (float)camera.Radius,
                    Rotation = camera.HorizontalRotation
                }
            };

            try
            {
                return dispatcher.Dispatch(View.Camera);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
    }
    #endregion

    #region Dimming
    /// <inheritdoc/>
    public override void DimDown()
    {
        // Make sure the sepia effect is available and not already active
        if (_sepiaShader != null)
        {
            // Gradually apply sepia effect
            _sepiaShader.Enabled = true;
            Engine.Interpolate(
                start: 0, target: 0.6,
                callback: value => _sepiaShader.Desaturation = _sepiaShader.Toning = (float)value,
                duration: 4);
        }

        base.DimDown();
    }

    /// <inheritdoc/>
    public override void DimUp()
    {
        if (_sepiaShader != null)
            _sepiaShader.Enabled = false;

        base.DimUp();
    }
    #endregion
}

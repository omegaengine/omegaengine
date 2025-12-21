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
using System.IO;
using AlphaFramework.Presentation;
using FrameOfReference.Presentation.Config;
using FrameOfReference.World;
using OmegaEngine;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using SlimDX;

namespace FrameOfReference.Presentation;

/// <summary>
/// Handles the visual representation of <see cref="World"/> content in the <see cref="OmegaEngine"/>
/// </summary>
public abstract partial class Presenter : CoordinatePresenter<Universe, Vector2>
{
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

    /// <summary>
    /// Creates a new presenter.
    /// </summary>
    /// <param name="engine">The engine to use for rendering.</param>
    /// <param name="universe">The game world to present.</param>
    protected Presenter(Engine engine, Universe universe) : base(engine, universe)
    {}

    /// <inheritdoc/>
    public override void Initialize()
    {
        if (Initialized) return;

        if (View.Lighting)
        {
            SetupLighting();
            UpdateLighting();
            Universe.LightingChanged += UpdateLighting;
        }
        SetupTerrain();

        base.Initialize();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        try
        {
            Universe.LightingChanged -= UpdateLighting;
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

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
            View.Lighting,
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
            pinPitch: 25, maxPitch: 60,
            heightController: CameraController)
        {
            Name = state.Name,
            Target = Universe.Terrain.ToEngineCoords(state.Position),
            Radius = state.Radius,
            Rotation = state.Rotation,
            FarClip = Universe.Fog ? Universe.FogDistance : 1e+6f
        };
    }

    /// <summary>
    /// The value for <see cref="ZoomCamera.MaxRadius"/>.
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
    /// <returns>The current state of the <see cref="Camera"/> or <c>null</c> if it cannot be determined at this time (e.g. transition in progress).</returns>
    public CameraState<Vector2>? CameraState
        => View.Camera switch
        {
            StrategyCamera camera => new()
            {
                Name = camera.Name,
                Position = camera.Target.Flatten(),
                Radius = camera.Radius,
                Rotation = camera.Rotation
            },
            _ => null
        };

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
}

using System;
using System.IO;
using AlphaFramework.Presentation;
using OmegaEngine;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using Template.AlphaFramework.Presentation.Config;
using Template.AlphaFramework.World;

namespace Template.AlphaFramework.Presentation;

/// <summary>
/// Handles the visual representation of <see cref="World"/> content in the <see cref="OmegaEngine"/>
/// </summary>
public abstract class Presenter : CoordinatePresenter<Universe, Vector2>
{
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

        if (Universe.Terrain != null) SetupTerrain();

        base.Initialize();
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
        if (Universe.Terrain == null) return;

        // Build texture array
        var textures = new string[Universe.Terrain.Templates.Length];
        for (int i = 0; i < textures.Length; i++)
        {
            var template = Universe.Terrain.Templates[i];
            if (template != null && !string.IsNullOrEmpty(template.Texture))
            {
                // Prefix directory name
                textures[i] = Path.Combine("Terrain", template.Texture);
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
        Scene.Positionables.Add(Terrain);
    }

    /// <summary>
    /// Creates a new camera based on a state usually loaded from the <see cref="Universe"/>.
    /// </summary>
    /// <param name="state">The state to place the new camera in; <c>null</c> for default.</param>
    /// <returns>The newly created <see cref="Camera"/>.</returns>
    protected Camera CreateCamera(CameraState<Vector2>? state = null)
    {
        if (state == null && Universe.Terrain != null)
        {
            state = new() {Name = "Main", Position = Universe.Terrain.Center, Radius = 1500};
        }
        state ??= new() {Name = "Main", Position = new Vector2(0, 0), Radius = 1500};

        var target = Universe.Terrain != null
            ? Universe.Terrain.ToEngineCoords(state.Position)
            : new DoubleVector3(state.Position.X, 0, state.Position.Y);

        return new StrategyCamera(CameraController)
        {
            Name = state.Name,
            Target = target,
            MinRadius = 200,
            MaxRadius = 2250,
            Radius = state.Radius,
            Rotation = state.Rotation,
            FarClip = 1e+6f
        };
    }

    /// <summary>
    /// Ensures the camera does not go under the <see cref="Terrain"/>.
    /// </summary>
    /// <returns>The minimum height the camera must have.</returns>
    protected virtual double CameraController(DoubleVector3 coordinates)
    {
        const float floatAboveGround = 100;
        if (Universe.Terrain != null)
            return Universe.Terrain.ToEngineCoords(coordinates.Flatten()).Y + floatAboveGround;
        return floatAboveGround;
    }

    /// <summary>
    /// Retrieves the current state of the <see cref="Camera"/> for storage in the <see cref="Universe"/>.
    /// </summary>
    /// <returns>The current state of the <see cref="Camera"/> or <c>null</c> if it cannot be determined at this time.</returns>
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
}

using System.Windows.Forms;
using AlphaFramework.Presentation;
using AlphaFramework.World.Positionables;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using Template.AlphaFramework.World;

namespace Template.AlphaFramework.Presentation;

/// <summary>
/// Handles the visual representation of <see cref="Universe"/> content in the <see cref="Engine"/>.
/// </summary>
/// <remarks>
/// The base <see cref="CoordinatePresenter{TUniverse,TCoordinates}"/> keeps the engine scene in sync with <see cref="Universe.Positionables"/>.
/// This class just builds a <see cref="Camera"/> and a <see cref="View"/> and maps each world model type to an engine renderable.
/// </remarks>
public abstract class Presenter(Engine engine, Universe universe)
    : CoordinatePresenter<Universe, Vector3>(engine, universe)
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        if (Initialized) return;
        base.Initialize();

        // Add a basic directional light so lit materials are visible.
        Scene.Lights.Add(new DirectionalLight {Name = "Sun", Direction = new(-0.6f, -1, -0.4f)});
    }

    /// <inheritdoc/>
    protected override void RegisterRenderablesSync()
        // Map each Positionable subtype to an engine renderable. Replace the placeholder sphere
        // with meshes appropriate for your own entities.
        => RenderablesSync.Register<Entity, Model>(
            _ => Model.Sphere(Engine, XTexture.Get(Engine, "flag.png")),
            UpdateRepresentation);

    /// <summary>
    /// Applies the position of a world model to its engine representation.
    /// </summary>
    protected static void UpdateRepresentation(Positionable<Vector3> element, IPositionable representation)
        => representation.Position = new DoubleVector3(element.Position.X, element.Position.Y, element.Position.Z);
}

/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// A <see cref="Model"/> that is rendered observing the <see cref="Camera"/>s view direction, but ignoring its position.
/// </summary>
/// <remarks>This kind of model effectively "floats" atop of the scene. This can be useful for things like direction widgets.</remarks>
/// <seealso cref="View.FloatingModels"/>
public class FloatingModel : Model
{
    #region Constructor

    #region Internal texture
    /// <summary>
    /// Creates a new floating model based upon a cached mesh, using its internal material data if available
    /// </summary>
    /// <param name="mesh">The mesh to use for rendering</param>
    public FloatingModel(XMesh mesh) : base(mesh)
    {}
    #endregion

    #region External texture
    /// <summary>
    /// Creates a new floating model based upon a cached mesh, using an external texture and a white material
    /// </summary>
    /// <param name="mesh">The mesh use for rendering</param>
    /// <param name="materials">The materials to use for rendering the mesh</param>
    public FloatingModel(XMesh mesh, params XMaterial[] materials)
        : base(mesh, materials)
    {}
    #endregion

    #region Custom mesh
    /// <summary>
    /// Creates a new floating model based upon a custom mesh. Normals should be already calculated if they are required.
    /// Warning: The custom mesh and materials will be automatically disposed!
    /// </summary>
    /// <param name="mesh">The mesh use for rendering</param>
    /// <param name="materials">The materials to use for rendering the mesh</param>
    public FloatingModel(Mesh mesh, params XMaterial[] materials)
        : base(mesh, materials)
    {}
    #endregion

    #endregion

    //--------------------//

    #region Render
    /// <inheritdoc/>
    internal override void Render(Camera camera, GetLights getLights = null)
    {
        // Note: Doesn't call base methods
        PrepareRender();

        // Set floating view transformation
        Matrix transform = Matrix.Scaling(Scale) * Matrix.RotationQuaternion(Rotation);
        switch (Billboard)
        {
            case BillboardMode.Spherical:
                Engine.State.WorldTransform = transform * Matrix.Translation((Vector3)Position);
                break;
            case BillboardMode.Cylindrical:
                Engine.State.WorldTransform = transform * camera.CylindricalBillboard * camera.SimpleView * Matrix.Translation((Vector3)Position);
                break;
            default:
                Engine.State.WorldTransform = transform * camera.SimpleView * Matrix.Translation((Vector3)Position);
                break;
        }

        // Never light a floating model
        SurfaceEffect = SurfaceEffect.Plain;

        var effectiveLights = (SurfaceEffect == SurfaceEffect.Plain || getLights == null)
            ? []
            : getLights(Position, BoundingSphere?.Radius ?? 0);
        for (int i = 0; i < NumberSubsets; i++) RenderSubset(i, camera, effectiveLights);
    }
    #endregion
}

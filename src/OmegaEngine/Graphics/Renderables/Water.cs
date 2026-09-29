/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Assets;
using OmegaEngine.Foundation.Light;
using OmegaEngine.Graphics.Shaders;
using OmegaEngine.Graphics.VertexDecl;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// Displays a water plane with reflections and refraction
/// </summary>
/// <remarks>The transparency and shader are determined by <see cref="EngineEffects.WaterEffects"/>; <see cref="Renderable.Alpha"/> and <see cref="PositionableRenderable.SurfaceShader"/> are ignored.</remarks>
public class Water : Model
{
    #region Variables
    private WaterViewSource? _viewSource;
    #endregion

    #region Properties
    /// <summary>
    /// The size of the water plane
    /// </summary>
    public SizeF Size { get; }
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new water plane.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="size">The size of the water plane.</param>
    public Water(Engine engine, SizeF size) : base(BuildMesh(engine, size), XMaterial.Default)
    {
        Engine = engine;
        Size = size;

        RenderIn = ViewType.NormalOnly;
        Pickable = false;
        Materials[0].Emissive = Color.LightBlue;

        BoundingBox = new(minimum: new(), maximum: new(size.Width, 0, -size.Height));
    }

    private static Mesh BuildMesh(Engine engine, SizeF size)
    {
        // Compensate for texture stretch (ignored by all but the simple shader)
        float tu = size.Width / 1500;
        float tv = size.Height / 1500;

        PositionNormalTextured[] vertexes =
        [
            new(new Vector3(0, 0, 0), new Vector3(0, 1, 0), 0, 0),
            new(new Vector3(size.Width, 0, 0), new Vector3(0, 1, 0), tu, 0),
            new(new Vector3(0, 0, -size.Height), new Vector3(0, 1, 0), 0, tv),
            new(new Vector3(size.Width, 0, -size.Height), new Vector3(0, 1, 0), tu, tv)
        ];
        short[] indexes = [0, 1, 3, 3, 2, 0];

        var mesh = new Mesh(engine.Device, indexes.Length / 3, vertexes.Length, MeshFlags.SystemMemory, PositionNormalTextured.Format);
        try
        {
            mesh.WriteVertexBuffer(vertexes);
            mesh.WriteIndexBuffer(indexes);
        }
        catch
        {
            mesh.Dispose();
            throw;
        }

        return mesh;
    }
    #endregion

    //--------------------//

    #region Setup
    /// <summary>
    /// Creates views as reflection and refraction sources - Call after setting the position and adding to a <see cref="Scene"/> or parent renderable!
    /// </summary>
    /// <param name="view">The original view to reflect</param>
    /// <param name="clipTolerance">How far to shift the clip plane along its normal vector to reduce graphical glitches at edges</param>
    /// <remarks>This method may be called only once on an instance</remarks>
    public void SetupChildViews(View view, float clipTolerance = 10)
    {
        #region Sanity checks
        if (view == null) throw new ArgumentNullException(nameof(view));
        if (_viewSource != null) throw new InvalidOperationException(Resources.CallMethodOnlyOnce);
        #endregion

        // Make sure the required views get rendered first
        _viewSource = WaterViewSource.FromEngine(Engine, WorldPosition.Y, view, clipTolerance);
        RequiredViews.Add(_viewSource.RefractedView);
        RequiredViews.Add(_viewSource.ReflectedView);

        // Make sure the shaders are ready for use (disposed by the view source)
        RegisterChild(_viewSource.RefractionOnlyShader, autoDispose: false);
        RegisterChild(_viewSource.RefractionReflectionShader, autoDispose: false);
    }
    #endregion

    #region Render
    /// <summary>
    /// The <see cref="EngineEffects.WaterEffects"/> <see cref="_appearance"/> was resolved for; <c>null</c> if it has not been resolved yet.
    /// </summary>
    private WaterEffectsType? _appearanceEffects;

    /// <summary>
    /// How to render the water surface, based on <see cref="EngineEffects.WaterEffects"/>.
    /// </summary>
    /// <remarks>Used instead of <see cref="Renderable.Alpha"/> and <see cref="PositionableRenderable.SurfaceShader"/>.</remarks>
    private (int Alpha, SurfaceShader Shader) _appearance;

    /// <inheritdoc/>
    internal override void Render(RenderContext context)
    {
        if (_viewSource == null) throw new InvalidOperationException($"Must call {nameof(SetupChildViews)} before rendering {nameof(Water)}.");

        // Only re-resolve when the effects settings change instead of every frame
        var waterEffects = Engine.Effects.WaterEffects;
        if (_appearanceEffects != waterEffects)
        {
            _appearance = GetAppearance(waterEffects, _viewSource);
            _appearanceEffects = waterEffects;
        }
        var (alpha, shader) = _appearance;

        // Note: Doesn't call base methods
        PrepareRender();
        Engine.State.AlphaBlend = alpha;
        Engine.State.WorldTransform = WorldTransform;

        // Transfer the reflection view matrix to the shader
        _viewSource.RefractionReflectionShader.ReflectionViewProjection = _viewSource.ReflectedView.Camera.ViewProjection;

        RenderHelper(() => Mesh.DrawSubset(0), Materials[0], context, shader, effectiveLights: []);
        if (DrawBoundingBox && WorldBoundingBox is {} box && GetSurfaceEffect(context, shader) < SurfaceEffect.Glow)
            Engine.DrawBoundingBox(box);
    }

    /// <summary>
    /// Determines how to render the water surface.
    /// </summary>
    private (int Alpha, SurfaceShader Shader) GetAppearance(WaterEffectsType waterEffects, WaterViewSource viewSource)
        => waterEffects switch
        {
            WaterEffectsType.None => (128, Engine.SimpleWaterShader),
            WaterEffectsType.RefractionOnly => (EngineState.Opaque, viewSource.RefractionOnlyShader),
            _ => (EngineState.Opaque, viewSource.RefractionReflectionShader)
        };

    /// <inheritdoc/>
    /// <remarks>Rendering this without a shader isn't possible (non-standard FVF).</remarks>
    private protected override SurfaceEffect GetSurfaceEffect(RenderContext context, SurfaceShader? shader)
        => RenderContext.GetSurfaceEffect(
            configured: SurfaceEffect < SurfaceEffect.Shader ? SurfaceEffect.Shader : SurfaceEffect,
            context.Pass, lighting: true, shaderAvailable: shader != null);
    #endregion

    //--------------------//

    #region Dispose
    /// <inheritdoc/>
    protected override void OnDispose()
    {
        try
        {
            foreach (XMaterial material in Materials)
                material.ReleaseReference();

            if (_viewSource != null)
            {
                // Remove this water plane from the view source dependency list
                _viewSource.ReleaseReference();

                // Remove the view source, if it is no longer required
                if (_viewSource.ReferenceCount == 0)
                {
                    _viewSource.Dispose();
                    Engine.WaterViewSources.Remove(_viewSource);
                }
            }
        }
        finally
        {
            base.OnDispose();
        }
    }
    #endregion
}

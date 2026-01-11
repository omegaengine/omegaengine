/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using JetBrains.Annotations;
using NanoByte.Common;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Shaders;
using SlimDX;

namespace OmegaEngine.Graphics.Renderables;

#region Enumerations
/// <seealso cref="PositionableRenderable.RenderIn"/>
public enum ViewType
{
    /// <summary>Render in all types of <see cref="View"/>s</summary>
    All,

    /// <summary>Do not render in <see cref="SupportView"/>s</summary>
    NormalOnly,

    /// <summary>Render only in <see cref="SupportView"/>s</summary>
    SupportOnly,

    /// <summary>Render only in <see cref="GlowView"/>s</summary>
    GlowOnly
};

/// <seealso cref="PositionableRenderable.SurfaceEffect"/>
public enum SurfaceEffect
{
    /// <summary>Apply no lighting or other surface effects</summary>
    Plain,

    /// <summary>Use the fixed-function pipeline for lighting</summary>
    FixedFunction,

    /// <summary>Apply <see cref="SurfaceShader"/> for lighting and other surface effects</summary>
    Shader,

    /// <summary>Output the glow colors without any lighting</summary>
    Glow,

    /// <summary>Output the depth (Z-buffer) value as a grayscale color</summary>
    Depth
}

/// <seealso cref="PositionableRenderable.Billboard"/>
public enum BillboardMode
{
    /// <summary>Don't apply a billboarding effect</summary>
    None,

    /// <summary>Apply a spherical billboarding effect (object will always face the camera)</summary>
    Spherical,

    /// <summary>Apply a cylindrical billboarding effect (object's X axis will always face the camera)</summary>
    Cylindrical
};
#endregion

/// <summary>
/// An object that can be <see cref="Render"/>ed at a specific <see cref="Position"/> in a <see cref="Scene"/>.
/// </summary>
/// <seealso cref="Scene.Positionables"/>
public abstract class PositionableRenderable : Renderable, IFloatingOriginAware
{
    #region Variables
    private Matrix
        _internalScaling = Matrix.Identity,
        _internalRotation = Matrix.Identity,
        _internalTranslation = Matrix.Identity;

    /// <summary>Does the world transform need to be recalculated?</summary>
    protected bool WorldTransformDirty = true;
    #endregion

    #region Properties

    #region Flags
    /// <summary>
    /// Shall this <see cref="PositionableRenderable"/> be pickable with the mouse?
    /// </summary>
    [DefaultValue(true), Description("Shall this body be pickable with the mouse?"), Category("Behavior")]
    public bool Pickable { get; set; } = true;

    /// <summary>
    /// In what kind of <see cref="View"/>s shall this body be rendered?
    /// </summary>
    [DefaultValue(ViewType.All), Description("In what kind of Views shall this body be rendered?"), Category("Behavior")]
    public ViewType RenderIn { get; set; }

    /// <summary>
    /// What kinds of surface effects (e.g. lighting) to apply to this <see cref="PositionableRenderable"/>
    /// </summary>
    [DefaultValue(SurfaceEffect.Shader), Description("Apply surface shading effects (e.g. lighting) to this body? - Affects both fixed-function pipeline and shaders"), Category("Appearance")]
    public SurfaceEffect SurfaceEffect { get; set; } = SurfaceEffect.Shader;

    /// <summary>
    /// The <see cref="SurfaceShader"/> to apply to the surface of this <see cref="PositionableRenderable"/>
    /// </summary>
    /// <seealso cref="SurfaceEffect"/>
    /// <remarks>Will NOT be disposed when <see cref="EngineElement.Dispose"/> is called.</remarks>
    [Description("The shader to apply to the surface of this body"), Category("Appearance")]
    public SurfaceShader? SurfaceShader
    {
        get => _surfaceShader;
        set
        {
            _surfaceShader = value;
            RegisterChild(_surfaceShader, autoDispose: false);
        }
    }

    /// <summary>
    /// How this <see cref="PositionableRenderable"/> shall be rotated towards the camera
    /// </summary>
    [DefaultValue(BillboardMode.None), Description("How this body shall be rotated towards the camera"), Category("Layout")]
    public BillboardMode Billboard { get; set; }

    /// <summary>
    /// Shall this object still be rendered, even if it is beyond the <see cref="Camera.FarClip"/> plane? (at the cost of incorrect Z-ordering)
    /// </summary>
    [DefaultValue(false), Description("Shall this object still be rendered, even if it is beyond the camera's far clip plane? (at the cost of incorrect Z-ordering)"), Category("Layout")]
    public bool PreventFarClipping { get; set; }

    /// <summary>
    /// Shall this <see cref="PositionableRenderable"/> cast shadows on other objects?
    /// </summary>
    [DefaultValue(false), Description("Shall this body cast shadows on other objects?"), Category("Behavior")]
    public bool ShadowCaster { get; set; }

    /// <summary>
    /// Shall this <see cref="PositionableRenderable"/> receive shadows from other objects?
    /// </summary>
    [DefaultValue(false), Description("Shall this body receive shadows from other objects?"), Category("Behavior")]
    public bool ShadowReceiver { get; set; }
    #endregion

    #region Transform factors
    private Matrix _preTransform = Matrix.Identity;

    /// <summary>
    /// A transformation matrix that is to be applied before the normal world transform occurs - useful for correcting off-center meshes
    /// </summary>
    [Browsable(false)]
    public Matrix PreTransform { get => _preTransform; set => value.To(ref _preTransform, ref WorldTransformDirty); }

    private Vector3 _scale = new(1, 1, 1);

    /// <summary>
    /// Scaling to be performed before rendering
    /// </summary>
    [Description("Scaling to be performed before rendering"), Category("Layout")]
    public Vector3 Scale { get => _scale; set => value.To(ref _scale, ref WorldTransformDirty); }

    /// <summary>
    /// Scales this <see cref="PositionableRenderable"/> symmetrically
    /// </summary>
    /// <param name="factor">The factor by which to scale</param>
    public void SetScale(float factor)
    {
        new Vector3(factor).To(ref _scale, ref WorldTransformDirty);
    }

    private Quaternion _rotation = Quaternion.Identity;

    /// <summary>
    /// The body's rotation quaternion
    /// </summary>
    [Browsable(false)]
    public Quaternion Rotation { get => _rotation; set => Quaternion.Normalize(value).To(ref _rotation, ref WorldTransformDirty); }

    private DoubleVector3 _position;

    /// <summary>
    /// The body's position in world space
    /// </summary>
    [Description("The body's position in world space"), Category("Layout")]
    public DoubleVector3 Position { get => _position; set => value.To(ref _position, ref WorldTransformDirty); }

    private DoubleVector3 _floatingOrigin;

    /// <summary>
    /// A value to be added to <see cref="Position"/> in order gain <see cref="IFloatingOriginAware.FloatingPosition"/> - auto-updated by <see cref="View.Render"/> to the negative <see cref="Camera.Position"/>
    /// </summary>
    DoubleVector3 IFloatingOriginAware.FloatingOrigin { get => _floatingOrigin; set => value.To(ref _floatingOrigin, ref WorldTransformDirty); }
    #endregion

    #region Transform results
    /// <summary>Called to generate/update transformation matrices and related values</summary>
    protected virtual void RecalcWorldTransform()
    {
        if (!WorldTransformDirty) return;

        _floatingPosition = this.ApplyFloatingOriginTo(_position);

        // Calculate transformation matrices
        WorldTransformCached = _preTransform * Matrix.Scaling(_scale) * _internalScaling * Matrix.RotationQuaternion(Rotation) * _internalRotation * Matrix.Translation(_floatingPosition) * _internalTranslation;
        _inverseWorldTransform = Matrix.Invert(WorldTransformCached);

        // Transform bounding bodies into world space
        _worldBoundingSphere = BoundingSphere?.Transform(WorldTransformCached);
        _worldBoundingBox = BoundingBox?.Transform(WorldTransformCached);

        WorldTransformDirty = false;
    }

    private Vector3 _floatingPosition;

    /// <summary>
    /// The body's position in render space, based on <see cref="Position"/>
    /// </summary>
    /// <remarks>Constantly changes based on the values set for <see cref="IFloatingOriginAware.FloatingPosition"/></remarks>
    Vector3 IFloatingOriginAware.FloatingPosition
    {
        get
        {
            RecalcWorldTransform();
            return _floatingPosition;
        }
    }

    protected Matrix WorldTransformCached { get; private set; }

    /// <summary>
    /// The world transformation matrix for this entity
    /// </summary>
    /// <remarks>Constantly changes based on the values set for <see cref="IFloatingOriginAware.FloatingPosition"/></remarks>
    internal Matrix WorldTransform
    {
        get
        {
            RecalcWorldTransform();
            return WorldTransformCached;
        }
    }

    private Matrix _inverseWorldTransform;

    /// <summary>
    /// The world transformation matrix for this entity
    /// </summary>
    /// <remarks>Constantly changes based on the values set for <see cref="IFloatingOriginAware.FloatingPosition"/></remarks>
    internal Matrix InverseWorldTransform
    {
        get
        {
            RecalcWorldTransform();
            return _inverseWorldTransform;
        }
    }
    #endregion

    #region Bounding bodies
    /// <summary>
    /// A sphere that completely encompasses the body (in entity space, even before apply <see cref="PreTransform"/>).
    /// </summary>
    [Browsable(false)]
    public BoundingSphere? BoundingSphere { get; protected set; }

    private BoundingSphere? _worldBoundingSphere;

    /// <summary>
    /// A sphere that completely encompasses the body (in world space, used for culling tests).
    /// </summary>
    [Browsable(false)]
    public BoundingSphere? WorldBoundingSphere
    {
        get
        {
            RecalcWorldTransform();
            return _worldBoundingSphere;
        }
    }

    /// <summary>
    /// Shall the bounding sphere used to cull this object be drawn/visualized? (used for debugging)
    /// </summary>
    [DefaultValue(false), Description("Shall the bounding sphere used to cull this object be drawn/visualized? (used for debugging)"), Category("Appearance")]
    public bool DrawBoundingSphere { get; set; }

    /// <summary>
    /// A axis-aligned box that completely encompasses the body (in entity space, even before apply <see cref="PreTransform"/>).
    /// </summary>
    [Browsable(false)]
    public BoundingBox? BoundingBox { get; protected set; }

    private BoundingBox? _worldBoundingBox;

    /// <summary>
    /// An axis-aligned box that completely encompasses the body (in world space, used for culling tests).
    /// </summary>
    [Browsable(false)]
    public BoundingBox? WorldBoundingBox
    {
        get
        {
            RecalcWorldTransform();
            return _worldBoundingBox;
        }
    }

    /// <summary>
    /// Shall the bounding box used to cull this object be drawn/visualized? (used for debugging)
    /// </summary>
    [DefaultValue(false), Description("Shall the bounding box used to cull this object be drawn/visualized? (used for debugging)"), Category("Appearance")]
    public bool DrawBoundingBox { get; set; }
    #endregion

    // Order is not important, duplicate entries are not allowed
    private readonly HashSet<View> _requiredViews = [];
    private SurfaceShader? _surfaceShader;

    /// <summary>
    /// A list of <see cref="View"/>s that must be rendered before this <see cref="PositionableRenderable"/> can be rendered
    /// </summary>
    [Browsable(false)]
    public ICollection<View> RequiredViews => _requiredViews;
    #endregion

    //--------------------//

    #region Render
    /// <inheritdoc/>
    internal override void Render(Camera camera, GetEffectiveLighting? getEffectiveLighting = null)
    {
        #region Sanity checks
        if (IsDisposed) throw new ObjectDisposedException(ToString());
        if (camera == null) throw new ArgumentNullException(nameof(camera));
        #endregion

        // Note: Assumes IsVisible() was called in this frame, which in turn triggered UpdateInternalTransformations()

        if (SurfaceEffect < SurfaceEffect.Glow)
            DrawBoundingBodies();

        // Fall back to plain rendering if lighting is disabled
        if (getEffectiveLighting == null && SurfaceEffect is SurfaceEffect.FixedFunction or SurfaceEffect.Shader)
            SurfaceEffect = SurfaceEffect.Plain;

        base.Render(camera, getEffectiveLighting);
    }

    private void UpdateInternalTransformations(Camera camera)
    {
        var internalScaling = Matrix.Identity;
        var internalTranslation = Matrix.Identity;
        if (PreventFarClipping)
        {
            float maxDistance = camera.FarClip - WorldBoundingSphere?.Radius ?? camera.FarClip * 0.1f;
            var relativePosition = camera.Position.ApplyOffset(Position);

            float distanceFromCamera = relativePosition.Length();
            if (distanceFromCamera > maxDistance)
            {
                float ratio = maxDistance / distanceFromCamera;
                internalTranslation = Matrix.Translation(relativePosition * (1.0f - ratio));
                internalScaling = Matrix.Scaling(new(ratio));
            }
        }
        internalScaling.To(ref _internalScaling, ref WorldTransformDirty);
        internalTranslation.To(ref _internalTranslation, ref WorldTransformDirty);

        (Billboard switch
        {
            BillboardMode.Spherical => camera.SphericalBillboard,
            BillboardMode.Cylindrical => camera.CylindricalBillboard,
            _ => Matrix.Identity
        }).To(ref _internalRotation, ref WorldTransformDirty);
    }

    private void DrawBoundingBodies()
    {
        if (DrawBoundingSphere && WorldBoundingSphere is {} sphere) Engine.DrawBoundingSphere(sphere);
        if (DrawBoundingBox && WorldBoundingBox is {} box) Engine.DrawBoundingBox(box);
    }
    #endregion

    #region Render helper
    /// <summary>
    /// Provides an automatic rendering framework that handles things like setting textures, materials, lighting and shaders.
    /// </summary>
    /// <param name="render">A delegate that will be called once per rendering pass to display the actual content.</param>
    /// <param name="material">The material to apply to everything rendered.</param>
    /// <param name="camera">The currently effective <see cref="Camera"/>.</param>
    /// <param name="effectiveLighting">The currently effective lighting information for the renderable's position.</param>
    protected void RenderHelper([InstantHandle] Action render, XMaterial material, Camera camera, EffectiveLighting effectiveLighting)
    {
        #region Sanity checks
        if (render == null) throw new ArgumentNullException(nameof(render));
        if (camera == null) throw new ArgumentNullException(nameof(camera));
        #endregion

        SetEngineState(material);
        SetEngineState(camera);

        switch (SurfaceEffect)
        {
            case SurfaceEffect.Plain:
                RenderPlain(render, material);
                break;
            case SurfaceEffect.Glow:
                RenderGlow(render, material);
                break;
            case SurfaceEffect.FixedFunction:
                RenderFixedFunction(render, material);
                break;
            case SurfaceEffect.Shader:
                RenderShader(render, material, camera, effectiveLighting.GetShadowedLightSources(this));
                break;
        }

        Engine.State.UserClipPlane = default;
    }

    private void SetEngineState(XMaterial material)
    {
        // Set texture
        Engine.State.SetTexture(material.DiffuseMaps[0]);

        // Fall back to fixed-function pipeline if no shader was set for this body
        if (SurfaceEffect == SurfaceEffect.Shader && SurfaceShader == null)
            SurfaceEffect = SurfaceEffect.FixedFunction;
    }

    private void SetEngineState(Camera camera)
    {
        // Activate user clip plane if it is set
        if (camera.ClipPlane != default)
        {
            Engine.State.UserClipPlane = (SurfaceEffect == SurfaceEffect.Shader)
                // Transform the user clip plane into camera space for rendering with shaders
                ? Plane.Transform(camera.EffectiveClipPlane, camera.ViewProjection)
                // When rendering without shaders the clip plane is in world space
                : camera.EffectiveClipPlane;
        }
    }

    private void RenderPlain([InstantHandle] Action render, XMaterial material)
    {
        using (new ProfilerEvent("Surface effect: None"))
        {
            if (material.Diffuse == Color.White)
            { // A plain white surface needs no lighting at all
                Engine.State.FfpLighting = false;
            }
            else
            { // Simulate a plain colored surface by using emissive lighting
                Engine.State.FfpLighting = true;
                Engine.Device.Material = new() {Emissive = material.Diffuse};
            }

            render();
        }
    }

    private void RenderGlow([InstantHandle] Action render, XMaterial material)
    {
        using (new ProfilerEvent("Surface effect: Glow"))
        {
            Engine.State.FfpLighting = true;
            Engine.State.SetTexture(material.GlowMap);
            Engine.Device.Material = new() {Emissive = material.Glow};

            // Alpha setting applies both to regular and glow rendering pass.
            // However, if there is no glow texture, alpha-channel blending does not work.
            if (material.GlowMap == null && Alpha is EngineState.AlphaChannel or EngineState.BinaryAlphaChannel)
                Engine.State.AlphaBlend = 0;

            render();
        }
    }

    private void RenderFixedFunction([InstantHandle] Action render, XMaterial material)
    {
        using (new ProfilerEvent("Surface effect: Fixed-function"))
        {
            Engine.State.FfpLighting = true;
            Engine.Device.Material = material.D3DMaterial;

            render();
        }
    }

    private void RenderShader([InstantHandle] Action render, XMaterial material, Camera camera, LightSource[] lights)
    {
        using (new ProfilerEvent("Surface effect: Shader"))
        {
            Engine.State.FfpLighting = false;

            if (SurfaceShader != null)
            {
                using (new ProfilerEvent(() => $"Apply {SurfaceShader}"))
                    SurfaceShader.Apply(render, material, camera, lights);
            }
        }
    }
    #endregion

    #region Visiblity check
    /// <summary>
    /// Checks whether this object is visible at the moment (includes Frustum Culling and other filtering criteria).
    /// </summary>
    /// <param name="camera">The <see cref="Camera"/> used to look the object.</param>
    /// <returns><c>true</c> if the object is visible.</returns>
    /// <seealso cref="Cameras.Camera.InFrustum(SlimDX.BoundingSphere)"/>
    /// <seealso cref="Cameras.Camera.InFrustum(SlimDX.BoundingBox)"/>
    internal bool IsVisible(Camera camera)
    {
        #region Sanity checks
        if (camera == null) throw new ArgumentNullException(nameof(camera));
        #endregion

        if (!Visible || Alpha == EngineState.Invisible)
            return false;

        // Ensure automatic scaling and transformation effects are applied to bounding bodies
        UpdateInternalTransformations(camera);

        if (WorldBoundingSphere is {} sphere)
        {
            if (!camera.AtLeastOnePixelWide(sphere)) return false;
            if (!PreventFarClipping && !camera.InFrustum(sphere)) return false;
        }
        if (WorldBoundingBox is {} box)
        {
            if (!PreventFarClipping && !camera.InFrustum(box)) return false;
        }

        return true;
    }
    #endregion

    #region Intersect test
    /// <summary>
    /// Determine whether this <see cref="PositionableRenderable"/> is intersected by a ray.
    /// </summary>
    /// <param name="ray">A ray in world space along which to check for intersections.</param>
    /// <param name="distance">Returns the distance along the <paramref name="ray"/> at which the intersection took place.</param>
    /// <returns><c>true</c> if this <see cref="PositionableRenderable"/> was intersected by the <paramref name="ray"/>.</returns>
    /// <seealso cref="View.Pick(Point)"/>
    public virtual bool Intersects(Ray ray, out float distance)
    {
        distance = float.MaxValue;
        return false;
    }

    /// <summary>
    /// Determine whether this <see cref="PositionableRenderable"/> is intersected by a ray.
    /// </summary>
    /// <param name="ray">A ray in world space along which to check for intersections.</param>
    /// <param name="position">Returns the position of the intersection in entity space.</param>
    /// <returns><c>true</c> if this <see cref="PositionableRenderable"/> was intersected by the <paramref name="ray"/>.</returns>
    /// <seealso cref="View.Pick(Point,out DoubleVector3)"/>
    public bool Intersects(Ray ray, out DoubleVector3 position)
    {
        bool result = Intersects(ray, out float distance);

        // Calculate position along the ray and compensate for floating origin
        position = (ray.Position + distance * ray.Direction) + this.GetFloatingOrigin();
        return result;
    }

    /// <summary>
    /// Determine whether <see cref="PositionableRenderable.WorldBoundingBox"/> and <see cref="PositionableRenderable.WorldBoundingSphere"/> (if defined) are intersected by a ray.
    /// </summary>
    /// <param name="ray">A ray in world space along which to check for intersections.</param>
    /// <returns><c>true</c> if the bounding bodies were undefined or intersected by the <paramref name="ray"/>.</returns>
    protected virtual bool IntersectsBounding(Ray ray)
    {
        if (WorldBoundingSphere.HasValue && !SlimDX.BoundingSphere.Intersects(WorldBoundingSphere.Value, ray, out float _))
            return false;
        if (WorldBoundingBox.HasValue && !SlimDX.BoundingBox.Intersects(WorldBoundingBox.Value, ray, out float _))
            return false;
        return true;
    }
    #endregion

    //--------------------//

    #region Engine
    /// <inheritdoc/>
    protected override void OnEngineSet()
    {
        base.OnEngineSet();
        SurfaceShader = Engine.DefaultShader;
    }
    #endregion
}

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
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Foundation.Light;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Shaders;
using SlimDX;

namespace OmegaEngine.Graphics.Renderables;

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

/// <summary>
/// An object that can be <see cref="Render"/>ed at a specific <see cref="Position"/> in a <see cref="Scene"/>.
/// </summary>
/// <seealso cref="Scene.Positionables"/>
public abstract class PositionableRenderable : Renderable, IFloatingOriginAware
{
    #region Hierarchy
    private readonly RenderableCollection _children;

    /// <summary>
    /// Creates a new positionable renderable.
    /// </summary>
    protected PositionableRenderable()
    {
        RegisterChild(_children = new(this));
    }

    /// <summary>
    /// The <see cref="PositionableRenderable"/>s placed in this one's local coordinate system.
    /// </summary>
    /// <remarks>
    /// <para>A renderable lives in exactly one collection at a time: either this or <see cref="Scene.Positionables"/>.
    /// Adding it here removes it from its previous collection and keeps its local <see cref="Position"/>, <see cref="Rotation"/>, <see cref="Scale"/> and <see cref="PreTransform"/>,
    /// i.e. its world position changes.</para>
    /// <para>Setting <see cref="Renderable.Visible"/> to <c>false</c> hides the entire subtree. A <see cref="View"/> skips the entire subtree if <see cref="SubtreeBoundingSphere"/> or <see cref="SubtreeBoundingBox"/> is outside its view frustum.</para>
    /// <para>Will be disposed when <see cref="EngineElement.Dispose"/> is called.</para>
    /// </remarks>
    [Browsable(false)]
    public ICollection<PositionableRenderable> Children => _children;

    /// <summary>
    /// <see cref="Children"/> with a non-allocating enumerator, for traversals that run every frame.
    /// </summary>
    internal RenderableCollection ChildCollection => _children;

    private RenderableCollection? _container;

    /// <summary>
    /// The collection this renderable is currently contained in; <c>null</c> if it is not part of a <see cref="Scene"/>.
    /// </summary>
    internal RenderableCollection? Container
    {
        get => _container;
        set
        {
            _container = value;
            MarkLocalTransformDirty();
        }
    }

    /// <summary>
    /// The renderable this one is placed in the coordinate system of; <c>null</c> if this is a root.
    /// </summary>
    private PositionableRenderable? Parent => _container?.Owner;

    /// <summary>
    /// The root of the hierarchy this renderable is part of; itself if it is a root.
    /// </summary>
    private PositionableRenderable Root
    {
        get
        {
            var node = this;
            while (node.Parent is {} parent) node = parent;
            return node;
        }
    }

    /// <summary>
    /// Determines whether this renderable is a (transitive) child of <paramref name="other"/>.
    /// </summary>
    internal bool IsDescendantOf(PositionableRenderable other)
    {
        for (var node = Parent; node != null; node = node.Parent)
            if (node == other) return true;
        return false;
    }
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

    private BillboardMode _billboard;

    /// <summary>
    /// How this <see cref="PositionableRenderable"/> shall be rotated towards the camera
    /// </summary>
    [DefaultValue(BillboardMode.None), Description("How this body shall be rotated towards the camera"), Category("Layout")]
    public BillboardMode Billboard { get => _billboard; set => value.To(ref _billboard, MarkDirty); }

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
    private Action? _markLocalTransformDirty;

    /// <summary>A cached delegate for <see cref="MarkLocalTransformDirty"/>, so that setters don't allocate one per call.</summary>
    private Action MarkDirty => _markLocalTransformDirty ??= MarkLocalTransformDirty;

    private Action? _markRenderTransformDirty;

    /// <summary>A cached delegate for <see cref="MarkRenderTransformDirty"/>, so that per-frame camera effect updates don't allocate one per call.</summary>
    private Action MarkRenderDirty => _markRenderTransformDirty ??= MarkRenderTransformDirty;

    private Matrix _preTransform = Matrix.Identity;
    private Matrix _billboardRotation = Matrix.Identity;
    private float _forcedPerspectiveScaling = 1;
    private DoubleVector3 _forcedPerspectiveTranslation;
    private float _autoScaleFactor = 1;

    /// <summary>
    /// A transformation matrix that is to be applied before the normal world transform occurs - useful for correcting off-center meshes
    /// </summary>
    [Browsable(false)]
    public Matrix PreTransform { get => _preTransform; set => value.To(ref _preTransform, MarkDirty); }

    private Vector3 _scale = new(1, 1, 1);

    /// <summary>
    /// Scaling to be performed before rendering
    /// </summary>
    /// <remarks>Inherited by <see cref="Children"/>; non-uniform scaling combined with a rotated child produces shear.</remarks>
    [Description("Scaling to be performed before rendering"), Category("Layout")]
    public Vector3 Scale { get => _scale; set => value.To(ref _scale, MarkDirty); }

    /// <summary>
    /// Scales this <see cref="PositionableRenderable"/> symmetrically
    /// </summary>
    /// <param name="factor">The factor by which to scale</param>
    public void SetScale(float factor)
    {
        new Vector3(factor).To(ref _scale, MarkDirty);
    }

    private Quaternion _rotation = Quaternion.Identity;

    /// <summary>
    /// The body's rotation quaternion
    /// </summary>
    [Browsable(false)]
    public Quaternion Rotation { get => _rotation; set => Quaternion.Normalize(value).To(ref _rotation, MarkDirty); }

    private DoubleVector3 _position;

    /// <summary>
    /// The body's position: absolute world space for a root, an offset in the parent's coordinate system for an element of another body's <see cref="Children"/>.
    /// </summary>
    /// <seealso cref="WorldPosition"/>
    [Description("The body's position - absolute world space for a root, relative to the parent for a child"), Category("Layout")]
    public DoubleVector3 Position { get => _position; set => value.To(ref _position, MarkDirty); }

    private DoubleVector3 _floatingOrigin;

    /// <summary>
    /// A value to be added to <see cref="Position"/> in order gain <see cref="IFloatingOriginAware.FloatingPosition"/> - auto-updated by <see cref="View.Render"/> to the negative <see cref="Camera.Position"/>
    /// </summary>
    /// <remarks>Only stored at the root of a hierarchy; descendants inherit it from there.</remarks>
    DoubleVector3 IFloatingOriginAware.FloatingOrigin
    {
        get => Root._floatingOrigin;
        set
        {
            var root = Root;
            value.To(ref root._floatingOrigin, root.MarkDirty);
        }
    }

    private float? _forcedPerspectiveDistance;

    /// <summary>
    /// When the renderable is farther than this distance from the <see cref="Camera"/>, it is instead rendered at this distance, with corresponding scaling applied to preserve its apparent size (angular diameter).
    /// </summary>
    /// <remarks>
    /// <para>A common usage pattern is to set this to a value slightly lower than <see cref="Camera.FarClip"/> to allow very large objects to remain visible in the distance.</para>
    /// <para>Z-order may appear incorrect if other objects are further than than this value from the camera and are close to this one in screen space.</para>
    /// </remarks>
    [Description("When the renderable is farther than this distance from the camera, it is instead rendered at this distance, with corresponding scaling applied to preserve its apparent size (angular diameter)."), Category("Layout")]
    public float? ForcedPerspectiveDistance { get => _forcedPerspectiveDistance; set => value.To(ref _forcedPerspectiveDistance, MarkDirty); }

    private float? _autoScaleDistance;

    /// <summary>
    /// The distance from the <see cref="Camera"/> beyond which the renderable is automatically scaled up to preserve its apparent size (angular diameter), keeping distant objects visible.
    /// </summary>
    /// <remarks>
    /// <para>While closer than this distance the renderable renders at its natural size; farther away it is scaled up so that it never appears smaller than it does at this distance.</para>
    /// <para>The auto-scaling is applied on top of <see cref="Scale"/> and is reflected in the bounding bodies used for culling. Combine with <see cref="ForcedPerspectiveDistance"/> for very large, very distant objects.</para>
    /// </remarks>
    [Description("The distance from the camera beyond which the renderable is automatically scaled up to preserve its apparent size (angular diameter), keeping distant objects visible."), Category("Layout")]
    public float? AutoScaleDistance { get => _autoScaleDistance; set => value.To(ref _autoScaleDistance, MarkDirty); }
    #endregion

    #region Transform results
    private uint _transformVersion, _parentTransformVersion;
    private bool _physicalTransformDirty = true, _renderTransformDirty = true;

    /// <summary>
    /// Invalidates all cached transforms of this body and makes its descendants observe the change.
    /// </summary>
    /// <remarks>Called by every mutation of the local transform factors and by every reparenting.</remarks>
    protected internal void MarkLocalTransformDirty()
    {
        _physicalTransformDirty = true;
        _renderTransformDirty = true;
        unchecked { _transformVersion++; }

        // A body's subtree bounds are dirty whenever any of its descendants' are, so the walk up can stop at the first ancestor that is already dirty
        _subtreeBoundsDirty = true;
        for (var node = Parent; node is {_subtreeBoundsDirty: false}; node = node.Parent)
            node._subtreeBoundsDirty = true;
    }

    /// <summary>
    /// Invalidates only the camera-dependent render transform of this body.
    /// </summary>
    /// <remarks>Used for the per-view camera effects, which apply to leaves only and therefore affect neither descendants nor <see cref="SubtreeBoundingSphere"/>/<see cref="SubtreeBoundingBox"/>.</remarks>
    private void MarkRenderTransformDirty()
        => _renderTransformDirty = true;

    private Matrix _physicalWorldTransform = Matrix.Identity;
    private DoubleVector3 _worldOrigin, _worldPosition;

    /// <summary>
    /// Ensures the physical transforms (i.e. without per-view camera effects) of this body and all its ancestors are up-to-date.
    /// </summary>
    private void EnsurePhysicalTransform()
    {
        var parent = Parent;
        if (parent != null)
        {
            parent.EnsurePhysicalTransform();

            // Observe changes further up the hierarchy and pass them on to our own descendants
            if (_parentTransformVersion != parent._transformVersion)
            {
                _parentTransformVersion = parent._transformVersion;
                MarkLocalTransformDirty();
            }
        }

        if (!_physicalTransformDirty) return;

        // Everything but the translation, i.e. the part that is unaffected by the floating origin
        var orientation = _preTransform * Matrix.Scaling(_scale) * Matrix.RotationQuaternion(_rotation);
        var localOrigin = new Vector3(orientation.M41, orientation.M42, orientation.M43);

        if (parent == null)
        {
            _physicalWorldTransform = orientation * Matrix.Translation(this.ApplyFloatingOriginTo(_position));
            _worldPosition = _position;
            _worldOrigin = _position + localOrigin;
        }
        else
        {
            _physicalWorldTransform = orientation * Matrix.Translation((Vector3)_position) * parent._physicalWorldTransform;
            _worldPosition = parent.ToWorld(_position);
            _worldOrigin = _worldPosition + (DoubleVector3)Vector3.TransformNormal(localOrigin, parent._physicalWorldTransform);
        }

        _physicalTransformDirty = false;
    }

    /// <summary>
    /// The body's absolute position in world space, resolved through the hierarchy in double precision.
    /// </summary>
    /// <remarks>Identical to <see cref="Position"/> for a root; independent of any <see cref="Camera"/> or floating origin.</remarks>
    [Browsable(false)]
    public DoubleVector3 WorldPosition
    {
        get
        {
            EnsurePhysicalTransform();
            return _worldPosition;
        }
    }

    /// <summary>
    /// Transforms an offset in this body's local coordinate system into an absolute position in world space.
    /// </summary>
    /// <param name="localOffset">The offset in the coordinate system that <see cref="Children"/> of this body live in.</param>
    /// <remarks>Independent of any <see cref="Camera"/> or floating origin.</remarks>
    internal DoubleVector3 ToWorld(DoubleVector3 localOffset)
    {
        EnsurePhysicalTransform();
        return _worldOrigin + (DoubleVector3)Vector3.TransformNormal((Vector3)localOffset, _physicalWorldTransform);
    }

    /// <summary>
    /// Ensures all cached transformation matrices and related values are up-to-date.
    /// </summary>
    protected void EnsureWorldTransform()
    {
        EnsurePhysicalTransform();
        if (!_renderTransformDirty) return;

        _floatingPositionCached = this.ApplyFloatingOriginTo(_worldPosition);

        // Camera effects apply to leaves only, so they never distort a body's descendants
        if (_children.Count == 0 && (_autoScaleFactor != 1 || _forcedPerspectiveScaling != 1 || !_billboardRotation.IsIdentity))
        {
            // Peel the body's own floating position off the physical transform, apply the camera effects around it and put it back on
            var centered = _physicalWorldTransform * Matrix.Translation(-_floatingPositionCached);

            WorldTransformWithoutForcedPerspectiveCached =
                centered
              * Matrix.Scaling(new(_autoScaleFactor))
              * _billboardRotation
              * Matrix.Translation(_floatingPositionCached);
            WorldTransformCached =
                centered
              * Matrix.Scaling(new(_autoScaleFactor * _forcedPerspectiveScaling))
              * _billboardRotation
              * Matrix.Translation(this.ApplyFloatingOriginTo(_worldPosition + _forcedPerspectiveTranslation));
        }
        else
        {
            WorldTransformCached = WorldTransformWithoutForcedPerspectiveCached = _physicalWorldTransform;
        }

        _inverseWorldTransform = Matrix.Invert(WorldTransformCached);
        _worldBoundingSphere = BoundingSphere?.Transform(WorldTransformWithoutForcedPerspectiveCached);
        _worldBoundingBox = BoundingBox?.Transform(WorldTransformWithoutForcedPerspectiveCached);

        _renderTransformDirty = false;
        RecalcWorldTransform();
    }

    /// <summary>
    /// Hook for recalculating renderable-specific values derived from the transformation matrices.
    /// </summary>
    /// <remarks>Called by <see cref="EnsureWorldTransform"/> after the matrices have been updated.</remarks>
    protected virtual void RecalcWorldTransform()
    {}

    private Vector3 _floatingPositionCached;

    /// <summary>
    /// The body's position in render space, based on <see cref="Position"/>
    /// </summary>
    /// <remarks>Constantly changes based on the values set for <see cref="IFloatingOriginAware.FloatingPosition"/></remarks>
    Vector3 IFloatingOriginAware.FloatingPosition
    {
        get
        {
            EnsureWorldTransform();
            return _floatingPositionCached;
        }
    }

    protected Matrix WorldTransformCached { get; private set; }

    protected Matrix WorldTransformWithoutForcedPerspectiveCached { get; private set; }

    /// <summary>
    /// The world transformation matrix for this entity, composed purely from the hierarchy's transforms.
    /// </summary>
    /// <remarks>Unlike <see cref="WorldTransformCached"/> this carries no billboarding, forced perspective or auto-scaling. Its translation is still relative to the floating origin.</remarks>
    protected Matrix PhysicalWorldTransform
    {
        get
        {
            EnsurePhysicalTransform();
            return _physicalWorldTransform;
        }
    }

    /// <summary>
    /// The world transformation matrix for this entity
    /// </summary>
    /// <remarks>Constantly changes based on the values set for <see cref="IFloatingOriginAware.FloatingPosition"/></remarks>
    internal Matrix WorldTransform
    {
        get
        {
            EnsureWorldTransform();
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
            EnsureWorldTransform();
            return _inverseWorldTransform;
        }
    }
    #endregion

    #region Bounding bodies
    private BoundingSphere? _boundingSphere, _worldBoundingSphere;

    /// <summary>
    /// A sphere that completely encompasses the body (in entity space, even before apply <see cref="PreTransform"/>).
    /// </summary>
    [Browsable(false)]
    public BoundingSphere? BoundingSphere
    {
        get => _boundingSphere;
        protected set => value.To(ref _boundingSphere, MarkDirty);
    }

    /// <summary>
    /// A sphere that completely encompasses the body (in floating world space, used for culling tests).
    /// </summary>
    [Browsable(false)]
    public BoundingSphere? WorldBoundingSphere
    {
        get
        {
            EnsureWorldTransform();
            return _worldBoundingSphere;
        }
    }

    /// <summary>
    /// Returns the <see cref="WorldBoundingSphere"/> or, if that is not available, a sphere with the floating world position and zero radius.
    /// </summary>
    protected BoundingSphere GetWorldBoundingSphereOrPosition()
        => WorldBoundingSphere ?? new(this.GetFloatingPosition(), radius: 0);

    /// <summary>
    /// Shall the bounding sphere used to cull this object be drawn/visualized? (used for debugging)
    /// </summary>
    [DefaultValue(false), Description("Shall the bounding sphere used to cull this object be drawn/visualized? (used for debugging)"), Category("Appearance")]
    public bool DrawBoundingSphere { get; set; }

    private BoundingBox? _boundingBox, _worldBoundingBox;

    /// <summary>
    /// An axis-aligned box that completely encompasses the body (in entity space, even before apply <see cref="PreTransform"/>).
    /// </summary>
    [Browsable(false)]
    public BoundingBox? BoundingBox
    {
        get => _boundingBox;
        protected set => value.To(ref _boundingBox, MarkDirty);
    }

    /// <summary>
    /// An axis-aligned box that completely encompasses the body (in floating world space, used for culling tests).
    /// </summary>
    [Browsable(false)]
    public BoundingBox? WorldBoundingBox
    {
        get
        {
            EnsureWorldTransform();
            return _worldBoundingBox;
        }
    }

    /// <summary>
    /// Shall the bounding box used to cull this object be drawn/visualized? (used for debugging)
    /// </summary>
    [DefaultValue(false), Description("Shall the bounding box used to cull this object be drawn/visualized? (used for debugging)"), Category("Appearance")]
    public bool DrawBoundingBox { get; set; }

    private bool _subtreeBoundsDirty = true, _subtreeCullable, _subtreeIgnoresFarClip;
    private BoundingSphere? _subtreeBoundingSphere;
    private BoundingBox? _subtreeBoundingBox;

    /// <summary>
    /// A sphere that completely encompasses this body and all its <see cref="Children"/> (in floating world space, used for culling whole subtrees).
    /// </summary>
    /// <remarks>
    /// <para><c>null</c> if the subtree cannot be culled as a whole, e.g. because it contains a body without bounding bodies or with <see cref="AutoScaleDistance"/>.</para>
    /// <para>Unlike <see cref="WorldBoundingSphere"/> this is independent of any <see cref="Camera"/>: <see cref="Billboard"/>ed leaves are covered by a sphere enclosing every possible rotation and <see cref="ForcedPerspectiveDistance"/> is not applied.</para>
    /// </remarks>
    [Browsable(false)]
    public BoundingSphere? SubtreeBoundingSphere
    {
        get
        {
            EnsureSubtreeBounds();
            return _subtreeBoundingSphere;
        }
    }

    /// <summary>
    /// An axis-aligned box that completely encompasses this body and all its <see cref="Children"/> (in floating world space, used for culling whole subtrees).
    /// </summary>
    /// <remarks>
    /// <para><c>null</c> if the subtree cannot be culled as a whole, e.g. because it contains a body without bounding bodies or with <see cref="AutoScaleDistance"/>.</para>
    /// <para>Unlike <see cref="WorldBoundingBox"/> this is independent of any <see cref="Camera"/>: <see cref="Billboard"/>ed leaves are covered by a box enclosing every possible rotation and <see cref="ForcedPerspectiveDistance"/> is not applied.</para>
    /// </remarks>
    [Browsable(false)]
    public BoundingBox? SubtreeBoundingBox
    {
        get
        {
            EnsureSubtreeBounds();
            return _subtreeBoundingBox;
        }
    }

    /// <summary>
    /// Indicates whether this body or any of its descendants uses <see cref="ForcedPerspectiveDistance"/>, so the subtree must not be culled by the far clip plane.
    /// </summary>
    internal bool SubtreeIgnoresFarClip
    {
        get
        {
            EnsureSubtreeBounds();
            return _subtreeIgnoresFarClip;
        }
    }

    /// <summary>
    /// Ensures <see cref="SubtreeBoundingSphere"/>, <see cref="SubtreeBoundingBox"/> and <see cref="SubtreeIgnoresFarClip"/> of this body and all its descendants are up-to-date.
    /// </summary>
    private void EnsureSubtreeBounds()
    {
        // May observe a transform change further up the hierarchy and thereby mark this body dirty
        EnsurePhysicalTransform();
        if (!_subtreeBoundsDirty) return;

        bool cullable = TryGetOwnSubtreeBounds(out var sphere, out var box);
        bool ignoreFarClip = _forcedPerspectiveDistance != null;

        foreach (var child in _children)
        {
            // Update every child even once the subtree is known to be uncullable, so no dirty descendant is left below a clean ancestor
            child.EnsureSubtreeBounds();
            ignoreFarClip |= child._subtreeIgnoresFarClip;
            if (!child._subtreeCullable) cullable = false;
            if (!cullable) continue;

            if (child._subtreeBoundingSphere is {} childSphere)
                sphere = sphere is {} ownSphere ? SlimDX.BoundingSphere.Merge(ownSphere, childSphere) : childSphere;
            if (child._subtreeBoundingBox is {} childBox)
                box = box is {} ownBox ? SlimDX.BoundingBox.Merge(ownBox, childBox) : childBox;
        }

        _subtreeCullable = cullable;
        _subtreeBoundingSphere = cullable ? sphere : null;
        _subtreeBoundingBox = cullable ? box : null;
        _subtreeIgnoresFarClip = ignoreFarClip;
        _subtreeBoundsDirty = false;
    }

    /// <summary>
    /// Determines the camera-independent bounds this body contributes to its own <see cref="SubtreeBoundingSphere"/> and <see cref="SubtreeBoundingBox"/>.
    /// </summary>
    /// <param name="sphere">The contributed sphere in floating world space; <c>null</c> if the body contributes nothing.</param>
    /// <param name="box">The contributed box in floating world space; <c>null</c> if the body contributes nothing.</param>
    /// <returns><c>false</c> if the body cannot be culled, so neither can any subtree containing it.</returns>
    private bool TryGetOwnSubtreeBounds(out BoundingSphere? sphere, out BoundingBox? box)
    {
        sphere = null;
        box = null;
        if (this is Pivot) return true; // Nothing to draw

        // Camera effects apply to leaves only
        bool isLeaf = _children.Count == 0;

        // The auto-scaling grows without bound as the camera moves away
        if (isLeaf && _autoScaleDistance != null) return false;

        sphere = _boundingSphere?.Transform(_physicalWorldTransform);
        box = _boundingBox?.Transform(_physicalWorldTransform);

        // IsVisible() never culls a body without any bounding bodies
        if (sphere == null && box == null) return false;

        // Fill in a missing bounding body from the other, so it does not disable culling for the entire subtree
        sphere ??= SlimDX.BoundingSphere.FromBox(box!.Value);
        box ??= SlimDX.BoundingBox.FromSphere(sphere.Value);

        if (isLeaf && _billboard != BillboardMode.None)
        {
            // The billboard rotation pivots on the body's floating position, so cover every possible rotation around it
            var pivot = this.ApplyFloatingOriginTo(_worldPosition);
            sphere = new SlimDX.BoundingSphere(pivot, radius: Vector3.Distance(sphere.Value.Center, pivot) + sphere.Value.Radius);
            box = SlimDX.BoundingBox.FromSphere(sphere.Value);
        }

        return true;
    }

    /// <summary>
    /// Checks whether this body and all its descendants can be skipped because <see cref="SubtreeBoundingSphere"/> and <see cref="SubtreeBoundingBox"/> are outside the <paramref name="camera"/>'s view frustum.
    /// </summary>
    /// <param name="camera">The <see cref="Camera"/> used to look at the subtree.</param>
    /// <returns><c>true</c> if any part of the subtree might be visible or the subtree cannot be culled as a whole.</returns>
    /// <remarks>Does not take <see cref="Renderable.Visible"/> or any other per-body filtering criteria into account.</remarks>
    internal bool SubtreeInFrustum(Camera camera)
    {
        EnsureSubtreeBounds();
        if (_subtreeBoundingSphere is not {} sphere || _subtreeBoundingBox is not {} box) return true;

        // Safe to use the pixel-size check: a sphere enclosed in another one never appears larger than the outer one
        return camera.AtLeastOnePixelWide(sphere)
            && camera.InFrustum(sphere, _subtreeIgnoresFarClip)
            && camera.InFrustum(box, _subtreeIgnoresFarClip);
    }
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
    internal override void Render(Camera camera, GetEffectiveLights? getEffectiveLights = null)
    {
        #region Sanity checks
        if (IsDisposed) throw new ObjectDisposedException(ToString());
        if (camera == null) throw new ArgumentNullException(nameof(camera));
        #endregion

        // Note: Assumes IsVisible() was called in this frame, which in turn triggered UpdateInternalTransformations()

        // Fall back to plain rendering if lighting is disabled
        if (getEffectiveLights == null && SurfaceEffect is SurfaceEffect.FixedFunction or SurfaceEffect.Shader)
            SurfaceEffect = SurfaceEffect.Plain;

        base.Render(camera, getEffectiveLights);

        if (SurfaceEffect < SurfaceEffect.Glow)
        {
            if (DrawBoundingSphere && WorldBoundingSphere is {} sphere) Engine.DrawBoundingSphere(sphere);
            if (DrawBoundingBox && WorldBoundingBox is {} box) Engine.DrawBoundingBox(box);
        }
    }

    private void UpdateInternalTransformations(Camera camera)
    {
        // Leaf-only effects; leaving the factors untouched on a parent avoids invalidating its whole subtree every frame
        if (_children.Count != 0) return;

        var relativePosition = camera.Position - WorldPosition;
        double distanceFromCamera = relativePosition.Length();

        // These factors change every frame and only feed the render transform, so they must not invalidate the physical transform or the ancestors' subtree bounds
        var (forcedPerspectiveScaling, forcedPerspectiveTranslation) = GetForcedPerspective(relativePosition, distanceFromCamera);
        forcedPerspectiveScaling.To(ref _forcedPerspectiveScaling, MarkRenderDirty);
        forcedPerspectiveTranslation.To(ref _forcedPerspectiveTranslation, MarkRenderDirty);

        GetAutoScale(distanceFromCamera).To(ref _autoScaleFactor, MarkRenderDirty);

        (Billboard switch
        {
            BillboardMode.Spherical => camera.SphericalBillboard,
            BillboardMode.Cylindrical => camera.CylindricalBillboard,
            _ => Matrix.Identity
        }).To(ref _billboardRotation, MarkRenderDirty);
    }

    private (float scaling, DoubleVector3 translation) GetForcedPerspective(DoubleVector3 relativePosition, double distanceFromCamera)
    {
        if (ForcedPerspectiveDistance is {} maxDistance && distanceFromCamera > maxDistance)
        {
            double ratio = maxDistance / distanceFromCamera;
            return (scaling: (float)ratio, translation: relativePosition * (1 - ratio));
        }

        return (scaling: 1, translation: new());
    }

    private float GetAutoScale(double distanceFromCamera)
        => AutoScaleDistance is {} startDistance
            ? (float)Math.Max(1, distanceFromCamera / startDistance)
            : 1;
    #endregion

    #region Render helper
    /// <summary>
    /// Provides an automatic rendering framework that handles things like setting textures, materials, lighting and shaders.
    /// </summary>
    /// <param name="render">A delegate that will be called once per rendering pass to display the actual content.</param>
    /// <param name="material">The material to apply to everything rendered.</param>
    /// <param name="camera">The currently effective <see cref="Camera"/>.</param>
    /// <param name="effectiveLights">The currently effective lighting information for the renderable's position.</param>
    protected void RenderHelper([InstantHandle] Action render, XMaterial material, Camera camera, IReadOnlyList<LightSource> effectiveLights)
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
                RenderFixedFunction(render, material, effectiveLights);
                break;
            case SurfaceEffect.Shader:
                RenderShader(render, material, camera, effectiveLights);
                break;
            case SurfaceEffect.Depth:
                RenderDepth(render, material, camera);
                break;
        }

        Engine.State.UserClipPlane = default;
    }

    private void SetEngineState(XMaterial material)
    {
        // Set texture
        Engine.State.SetTexture(material.DiffuseMap);
        Engine.State.SrgbTexture = true; // Diffuse maps hold color data

        // Fall back to fixed-function pipeline if no shader was set for this body
        if (SurfaceEffect == SurfaceEffect.Shader && SurfaceShader == null)
            SurfaceEffect = SurfaceEffect.FixedFunction;
    }

    private void SetEngineState(Camera camera)
    {
        // Activate user clip plane if it is set.
        // Only do this for shader-based rendering (transformed into camera space):
        // Mixing fixed-function draws (world-space plane) and shader draws (clip-space plane) makes drivers mis-clip the first shader draw after a fixed-function one, wiping out entire draw calls.
        // Bodies rendered without shaders are still culled against the clip plane at the body level by the view frustum check.
        if (camera.ClipPlane != default && SurfaceEffect == SurfaceEffect.Shader)
            Engine.State.UserClipPlane = Plane.Transform(camera.EffectiveClipPlane, camera.ViewProjection);
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
                Engine.Device.Material = new() {Emissive = material.Diffuse.SrgbToLinear()};
            }

            render();
        }
    }

    private void RenderGlow([InstantHandle] Action render, XMaterial material)
    {
        using (new ProfilerEvent("Surface effect: Glow"))
        {
            Engine.State.FfpLighting = true;
            Engine.Device.Material = new() {Emissive = material.Glow.SrgbToLinear()};

            if (material.Glow.EqualsIgnoreAlpha(Color.Black) && material.GlowMap == null
             && Alpha is EngineState.AlphaChannel or EngineState.BinaryAlphaChannel)
            {
                // A non-glowing, alpha-blended or alpha-tested surface:
                // emit black but keep its alpha channel (from the diffuse map) so opaque parts occlude the glow while transparent parts let it shine through
                Engine.State.SetTexture(material.DiffuseMap);
                Engine.State.SrgbTexture = true;
            }
            else
            {
                Engine.State.SetTexture(material.GlowMap);
                Engine.State.SrgbTexture = true; // Glow maps hold color data

                // Alpha setting applies both to regular and glow rendering pass.
                // However, if there is no glow texture, alpha-channel blending does not work.
                if (material.GlowMap == null && Alpha is EngineState.AlphaChannel or EngineState.BinaryAlphaChannel)
                    Engine.State.AlphaBlend = 0;
            }

            render();
        }
    }

    private void RenderFixedFunction([InstantHandle] Action render, XMaterial material, IReadOnlyList<LightSource> lights)
    {
        using (new ProfilerEvent("Surface effect: Fixed-function"))
        {
            Engine.State.FfpLighting = true;
            Engine.Device.Material = material.ToD3DMaterial();

            int lightCounter = 0;
            foreach (var light in lights)
            {
                if (lightCounter >= Engine.Device.Capabilities.MaxActiveLights) break;
                Engine.Device.SetLight(lightCounter, light.ToFfpLight());
                Engine.Device.EnableLight(lightCounter, true);
                lightCounter++;
            }

            render();

            for (int i = 0; i < lightCounter; i++)
                Engine.Device.EnableLight(i, false);
        }
    }

    private void RenderShader([InstantHandle] Action render, XMaterial material, Camera camera, IReadOnlyList<LightSource> lights)
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

    private void RenderDepth([InstantHandle] Action render, XMaterial material, Camera camera)
    {
        using (new ProfilerEvent("Surface effect: Depth"))
        {
            // Render a flat black surface and let fog fade it to white towards the far clip plane,
            // turning the near-black/far-white fog gradient into a grayscale depth visualization
            Engine.State.FfpLighting = true;
            Engine.Device.Material = new() {Emissive = Color.Black};
            Engine.State.SetTexture(null);

            bool fog = Engine.State.Fog;
            Color fogColor = Engine.State.FogColor;
            float fogStart = Engine.State.FogStart, fogEnd = Engine.State.FogEnd;
            bool srgbWrite = Engine.State.SrgbWrite, srgbTexture = Engine.State.SrgbTexture;

            Engine.State.Fog = true;
            Engine.State.FogColor = Color.White;
            Engine.State.FogStart = camera.NearClip;
            Engine.State.FogEnd = camera.FarClip;

            // The output is a normalized depth value, not a color
            Engine.State.SrgbWrite = false;
            Engine.State.SrgbTexture = false;

            render();

            Engine.State.Fog = fog;
            Engine.State.FogColor = fogColor;
            Engine.State.FogStart = fogStart;
            Engine.State.FogEnd = fogEnd;
            Engine.State.SrgbWrite = srgbWrite;
            Engine.State.SrgbTexture = srgbTexture;
        }
    }
    #endregion

    #region Visiblity check
    /// <summary>
    /// Triggered before checking the entity for visibility.
    /// Will be triggered multiple times if the entity is rendered in multiple views.
    /// </summary>
    public event Action? PreVisibilityCheck;

    /// <summary>
    /// Called before checking the entity for visibility.
    /// Will be called multiple times if the entity is rendered in multiple views.
    /// </summary>
    internal void OnPreVisibilityCheck()
        => PreVisibilityCheck?.Invoke();

    /// <summary>
    /// Checks whether this object is visible at the moment (includes Frustum Culling and other filtering criteria).
    /// </summary>
    /// <param name="camera">The <see cref="Camera"/> used to look the object.</param>
    /// <returns><c>true</c> if the object is visible.</returns>
    /// <remarks>Only checks this body itself. A <see cref="View"/> additionally skips the entire subtree of a body that is hidden (<see cref="Renderable.Visible"/>) or fails <see cref="SubtreeInFrustum"/>, without calling this for any of its descendants.</remarks>
    /// <seealso cref="Cameras.Camera.InFrustum(SlimDX.BoundingSphere,bool)"/>
    /// <seealso cref="Cameras.Camera.InFrustum(SlimDX.BoundingBox,bool)"/>
    internal bool IsVisible(Camera camera)
    {
        #region Sanity checks
        if (camera == null) throw new ArgumentNullException(nameof(camera));
        #endregion

        if (!Visible || Alpha == EngineState.Invisible)
            return false;

        // Ensure automatic scaling and transformation effects are applied to bounding bodies
        UpdateInternalTransformations(camera);

        bool ignoreFarClip = ForcedPerspectiveDistance != null;
        if (WorldBoundingSphere is {} sphere)
        {
            if (!camera.AtLeastOnePixelWide(sphere)) return false;
            if (!camera.InFrustum(sphere, ignoreFarClip)) return false;
        }
        if (WorldBoundingBox is {} box)
        {
            if (!camera.InFrustum(box, ignoreFarClip)) return false;
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
        distance = float.PositiveInfinity;
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
        if (WorldBoundingSphere is {} sphere && !SlimDX.BoundingSphere.Intersects(sphere, ray, out float _))
            return false;
        if (WorldBoundingBox is {} box && !SlimDX.BoundingBox.Intersects(box, ray, out float _))
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

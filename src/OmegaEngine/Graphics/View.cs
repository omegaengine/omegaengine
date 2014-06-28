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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using NanoByte.Common.Dispatch;
using NanoByte.Common.Utils;
using NanoByte.Common.Values;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Graphics.Shaders;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics
{
    /// <summary>
    /// Controls the rendering of a <see cref="OmegaEngine.Graphics.Scene"/> using a <see cref="Cameras.Camera"/>.
    /// </summary>
    /// <remarks>Multiple <see cref="View"/>s can share the same <see cref="OmegaEngine.Graphics.Scene"/>, but they should all have separate <see cref="Cameras.Camera"/>s.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public partial class View : EngineElement, IResetable
    {
        #region Variables

        #region Flags
        private bool _visible = true;
        private bool _lighting = true;
        #endregion

        #region Content
        private readonly bool _disposeScene;

        /// <summary>
        /// A <see cref="CinematicCamera"/> for smooth transitioning to <see cref="_targetCamera"/>
        /// </summary>
        /// <remarks>Must be <see langword="null"/> or equal to <see cref="Camera"/></remarks>
        private CinematicCamera _cinematicCamera;

        /// <summary>
        /// A new <see cref="Camera"/> to use after a transition is done
        /// </summary>
        /// <remarks>Must be <see langword="null"/> if <see cref="_cinematicCamera"/> is <see langword="null"/>, will replace <see cref="Camera"/></remarks>
        private Camera _targetCamera;
        #endregion

        #region Background
        /// <summary>Does the background qaud need to be recreated?</summary>
        protected bool BackgroundQuadDirty = true;
        #endregion

        #region Render-to-texture
        /// <summary>An alternative surface to render onto instead of the back-buffer</summary>
        protected RenderTarget RenderTarget;

        private RenderTarget _secondaryRenderTarget;

        private Viewport _viewport;
        #endregion

        #endregion

        #region Properties
        /// <summary>
        /// Was this view rendered in the this frame?
        /// </summary>
        /// <remarks>Used to debug culling methods</remarks>
        [Description("Was this view rendered in the this frame?"), Category("Appearance")]
        public bool RenderedInLastFrame { get; private set; }

        #region Name
        /// <summary>
        /// Text value to make it easier to identify a particular view
        /// </summary>
        [Description("Text value to make it easier to identify a particular view"), Category("Design")]
        public string Name { get; set; }

        public override string ToString()
        {
            string value = GetType().Name;
            if (!string.IsNullOrEmpty(Name))
                value += ": " + Name;
            return value;
        }
        #endregion

        #region Area
        private Rectangle _area;

        /// <summary>
        /// The screen area this view should fill (all zero for fullscreen)
        /// </summary>
        [Description("The screen area this view should fill (all zero for fullscreen)"), Category("Layout")]
        public Rectangle Area
        {
            get { return _area; }
            set
            {
                #region Sanity checks
                if (value.X < 0 || value.Y < 0 || value.Width < 0 || value.Height < 0)
                    throw new ArgumentOutOfRangeException("value");
                #endregion

                if (_area.Size != value.Size)
                { // If the size of the area changes, the render targets will need to be recreated
                    if (RenderTarget != null)
                    {
                        RenderTarget.Dispose();
                        RenderTarget = null;
                    }

                    if (_secondaryRenderTarget != null)
                    {
                        _secondaryRenderTarget.Dispose();
                        _secondaryRenderTarget = null;
                    }
                }

                value.To(ref _area, UpdateViewport);
            }
        }

        /// <summary>
        /// The center point of <see cref="Area"/>
        /// </summary>
        [Browsable(false)]
        public Point AreaCenter
        {
            get
            {
                return new Point(
                    _area.Location.X + _area.Size.Width / 2,
                    _area.Location.Y + _area.Size.Height / 2);
            }
        }
        #endregion

        #region Flags
        /// <summary>
        /// Shall the scene be rendered?
        /// </summary>
        [Description("Shall the scene be rendered?"), Category("Appearance")]
        public bool Visible { get { return _visible; } set { _visible = value; } }

        /// <summary>
        /// Cull clockwise instead of counter-clockwise?
        /// </summary>
        [Description("Cull clockwise instead of counter-clockwise?"), Category("Behavior")]
        public bool InvertCull { get; set; }

        /// <summary>
        /// The level of transparency from 0 (solid) to 255 (invisible) for the complete scene
        /// </summary>
        [Description("The level of transparency from 0 (solid) to 255 (invisible) for the complete scene"), Category("Appearance")]
        public virtual int FullAlpha { get; set; }

        /// <summary>
        /// Automatically apply a fog using <see cref="BackgroundColor"/> and the <see cref="Camera"/>s clip planes
        /// </summary>
        [Description("Automatically apply a fog using the background color and the camera clip planes"), Category("Appearance")]
        public virtual bool Fog { get; set; }

        /// <summary>
        /// Is lighting used in <see cref="RenderScene"/>?
        /// </summary>
        [Description("Is lighting used in rendering?"), Category("Appearance")]
        public virtual bool Lighting { get { return _lighting; } set { _lighting = value; } }

        /// <summary>
        /// Does this <see cref="View"/> render to a texture <see cref="RenderTarget"/>? Only <see langword="true"/> for <see cref="TextureView"/>s.
        /// </summary>
        protected virtual bool TextureRenderTarget { get { return false; } }
        #endregion

        #region Content
        private readonly EngineElementCollection<TextureView> _childViews = new EngineElementCollection<TextureView>();

        /// <summary>
        /// A list of <see cref="TextureView"/>s that are to be <see cref="Render"/>ed before this <see cref="View"/>.
        /// Usually only <see cref="Render"/>ed if a <see cref="PositionableRenderable"/> in <see cref="OmegaEngine.Graphics.Scene.Positionables"/> has it listed in <see cref="PositionableRenderable.RequiredViews"/>.
        /// </summary>
        /// <remarks>Will be disposed when <see cref="EngineElement.Dispose"/> is called.</remarks>
        [Browsable(false)]
        public ICollection<TextureView> ChildViews { get { return _childViews; } }

        private Color _backgroundColor = Color.Black;

        /// <summary>
        /// The background color and fog color of this scene - <see cref="Color.Empty"/> for no background, may use alpha-channel if <see cref="FullAlpha"/> is not set
        /// </summary>
        [Description("The background color and fog color of this scene - Color.Empty for no background, may use alpha-channel if FullAlpha is not set"), Category("Appearance")]
        public Color BackgroundColor { set { value.To(ref _backgroundColor, ref BackgroundQuadDirty); } get { return _backgroundColor; } }

        private readonly Scene _scene;

        /// <summary>
        /// The scene containing the <see cref="PositionableRenderable"/>s to be rendered.
        /// </summary>
        /// <remarks>Will NOT be disposed when <see cref="EngineElement.Dispose"/> is called.</remarks>
        [Browsable(false)]
        public Scene Scene { get { return _scene; } }

        private readonly EngineElementCollection<FloatingModel> _floatingModels = new EngineElementCollection<FloatingModel>();

        /// <summary>
        /// A list of <see cref="FloatingModel"/>s to be overlayed on top of the <see cref="Scene"/>. Use this for UI-like elements, e.g. axis-arrows.
        /// </summary>
        /// <remarks>Will be disposed when <see cref="EngineElement.Dispose"/> is called.</remarks>
        [Browsable(false)]
        public ICollection<FloatingModel> FloatingModels { get { return _floatingModels; } }

        private readonly EngineElementCollection<PostShader> _postShaders = new EngineElementCollection<PostShader>();

        /// <summary>
        /// A list of post-processing shaders to be applied after rendering the scene
        /// </summary>
        /// <remarks>Will be disposed when <see cref="EngineElement.Dispose"/> is called.</remarks>
        [Browsable(false)]
        public ICollection<PostShader> PostShaders { get { return _postShaders; } }

        /// <summary>
        /// The camera describing how to look at the scene
        /// </summary>
        [Browsable(false)]
        public Camera Camera { get; set; }
        #endregion

        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new view for rendering
        /// </summary>
        /// <param name="scene">The scene containing the <see cref="PositionableRenderable"/>s to be rendered. Will NOT be disposed when <see cref="EngineElement.Dispose"/> is called.</param>
        /// <param name="camera">The <see cref="Camera"/> to look at the <see cref="Scene"/> with</param>
        /// <param name="area">The screen area this view should fill (leave empty for fullscreen)</param>
        public View(Scene scene, Camera camera, Rectangle area = new Rectangle())
        {
            #region Sanity checks
            if (scene == null) throw new ArgumentNullException("scene");
            if (camera == null) throw new ArgumentNullException("camera");
            if (area.X < 0 || area.Y < 0 || area.Width < 0 || area.Height < 0)
                throw new ArgumentOutOfRangeException("area");
            #endregion

            RegisterChild(_childViews);
            RegisterChild(_floatingModels);
            RegisterChild(_postShaders);

            RegisterChild(_scene = scene, autoDispose: false);
            _area = area;
            Camera = camera;
        }

        /// <summary>
        /// Creates a new view for rendering a plain color fullscreen
        /// </summary>
        /// <param name="color">The plain color to render</param>
        public View(Color color) : this(new Scene(), new TrackCamera(1, 10))
        {
            _backgroundColor = color;
            _disposeScene = true;
        }
        #endregion

        //--------------------//

        #region Handle child views
        /// <summary>
        /// Handles the rendering of the <see cref="ChildViews"/>, intelligently filtering out those that aren't needed
        /// </summary>
        private void HandleChildViews()
        {
            new PerTypeDispatcher<TextureView>(ignoreMissing: false)
            {
                (GlowView glowView) => { if (Engine.Effects.PostScreenEffects) glowView.Render(); },
                (ShadowView shadowView) => { if (Engine.Effects.Shadows) shadowView.Render(); },
                (WaterView waterView) =>
                {
                    var effectLevel = waterView.Reflection ? WaterEffectsType.ReflectTerrain : WaterEffectsType.RefractionOnly;
                    if (Engine.Effects.WaterEffects >= effectLevel &&
                        _sortedWaters.Exists(water => water.RequiredViews.Contains(waterView)))
                    {
                        waterView.Fog = Fog;
                        waterView.Render();
                    }
                },
                (LazyView lazyView) =>
                {
                    lazyView.Fog = Fog;
                    if (_sortedOpaqueBodies.Exists(body => body.RequiredViews.Contains(lazyView)) ||
                        _sortedTransparentBodies.Exists(body => body.RequiredViews.Contains(lazyView)))
                        lazyView.Render();
                }
            }.Dispatch(_childViews.Where(childView => childView.Visible && !childView.IsDisposed));
        }
        #endregion

        #region Reset
        /// <summary>
        /// Is to be called at the beginning of a frame.
        /// </summary>
        void IResetable.Reset()
        {
            RenderedInLastFrame = false;
        }
        #endregion

        #region Picking
        /// <summary>
        /// Generates a 3D picking ray.
        /// </summary>
        /// <param name="location">The screen space location to start the ray from (usually mouse coordinates)</param>
        /// <returns>A ray in world space starting at the <see cref="Camera"/> position moving towards the specified <paramref name="location"/>.</returns>
        public Ray PickingRay(Point location)
        {
            Matrix m = Camera.ViewInverse;

            // The point of origin is the camera position in render space
            var point = new Vector3(m.M41, m.M42, m.M43);

            // Determine the screen space ray direction
            var direction = new Vector3(
                ((2.0f * (location.X - _viewport.X) / _viewport.Width) - 1) / Camera.Projection.M11,
                ((-2.0f * (location.Y - _viewport.Y) / _viewport.Height) + 1) / Camera.Projection.M22,
                1.0f);

            // Transform from screen space into world space
            direction = new Vector3(
                direction.X * m.M11 + direction.Y * m.M21 + direction.Z * m.M31,
                direction.X * m.M12 + direction.Y * m.M22 + direction.Z * m.M32,
                direction.X * m.M13 + direction.Y * m.M23 + direction.Z * m.M33);

            return new Ray(point, direction);
        }

        /// <summary>
        /// Pick an <see cref="PositionableRenderable"/> in 3D-space using the mouse
        /// </summary>
        /// <param name="location">The screen space location to start the ray from (usually mouse coordinates)</param>
        /// <param name="position">Returns the position of the vertex closest to the intersection in entity space</param>
        /// <returns>The picked <see cref="PositionableRenderable"/> or <see langword="null"/>.</returns>
        public PositionableRenderable Pick(Point location, out DoubleVector3 position)
        {
            Ray pickingRay = PickingRay(location);

            PositionableRenderable closestBody = null;
            float closestDistance = float.MaxValue;
            foreach (PositionableRenderable body in _sortedBodies)
            {
                // Find bodies that intersect the ray
                float distance;
                if (body.Pickable && body.Intersects(pickingRay, out distance))
                {
                    // Check if new hit is closer that the previous best
                    if (distance < closestDistance)
                    {
                        closestBody = body;
                        closestDistance = distance;
                    }
                }
            }

            if (closestBody == null) position = default(DoubleVector3);
            else
            {
                // Calculate position along the ray and compensate for position offset
                position = (pickingRay.Position + closestDistance * pickingRay.Direction) - ((IPositionableOffset)closestBody).Offset;
            }
            return closestBody;
        }
        #endregion

        //--------------------//

        #region Camera effects
        /// <summary>
        /// Switches from the current camera view to a new view using a cinematic effect
        /// </summary>
        /// <param name="target">The new camera</param>
        /// <param name="duration">The complete transition time in seconds</param>
        public void SwingCameraTo(Camera target, float duration = 2)
        {
            #region Sanity checks
            if (target == null) throw new ArgumentNullException("target");
            #endregion

            // Create the transitional camera and make it active
            Camera = _cinematicCamera = new CinematicCamera(
                sourcePosition: Camera.Position,
                targetPosition: target.Position,
                sourceQuat: Quaternion.RotationMatrix(Camera.View),
                targetQuat: Quaternion.RotationMatrix(target.View),
                duration: duration,
                engine: Engine)
            {
                Name = ("Swing to: " + target.Name),
                NearClip = Camera.NearClip,
                FarClip = Camera.FarClip
            };
            _targetCamera = target;
        }
        #endregion

        #region Generate special support views

        #region Glow
        private bool _glowSetup;

        /// <summary>
        /// Creates a glow-map of this view and adds it to <see cref="ChildViews"/>
        /// </summary>
        /// <param name="blurStrength">How strongly to blur the glow map - values between 0 and 10</param>
        /// <param name="glowStrength">A factor by which the blurred glow color is multiplied - values between 0 and 100</param>
        /// <remarks>Calling this method multiple times will have no effect</remarks>
        public void SetupGlow(float blurStrength = 3, float glowStrength = 1.5f)
        {
            // Only setup glow once
            if (_glowSetup) return;
            _glowSetup = true;

            // Create the new view, make sure the camera stays in sync, copy default properties
            var newView = new GlowView(this) {Name = Name + " Glow"};

            // Apply PostScreen shader to current scene (hooked with output from new scene)
            PostShaders.Add(new PostGlowShader(newView) {BlurStrength = blurStrength, GlowStrength = glowStrength});

            _childViews.Add(newView);
        }
        #endregion

        #region Shadow
        /// <summary>
        /// Creates a shadow-map for a <see cref="LightSource"/> in the <see cref="Scene"/>
        /// </summary>
        /// <param name="light">The <see cref="LightSource"/> to create the shadow-map for</param>
        public void SetupShadow(LightSource light)
        {
            // ToDo: Finish implementation

            // Create the new view, make sure the camera stays in sync, copy default properties
            var newView = new ShadowView(this)
            {
                Name = Name + " Shadow"
            };

            _childViews.Add(newView);
        }
        #endregion

        #endregion

        //--------------------//

        #region Engine
        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            base.OnEngineSet();

            // Calculate the effective viewport and continuously update it, if it is fullscreen
            UpdateViewport();
            if (_area == Rectangle.Empty) Engine.DeviceReset += UpdateViewport;
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        protected override void OnDispose()
        {
            try
            {
                // Unhook device events
                Engine.DeviceReset -= UpdateViewport;

                if (_disposeScene) Scene.Dispose();

                if (RenderTarget != null) RenderTarget.Dispose();
                if (_secondaryRenderTarget != null) _secondaryRenderTarget.Dispose();

                // Dispose and remove all water view sources associated to this view
                Engine.WaterViewSources.RemoveWhere(viewSource =>
                {
                    if (viewSource.RefractedView == this || viewSource.RefractedView == this)
                    {
                        viewSource.Dispose();
                        return true;
                    }
                    return false;
                });
            }
            finally
            {
                base.OnDispose();
            }
        }
        #endregion
    }
}

/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using NanoByte.Common.Values;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Values;

namespace OmegaEngine.Graphics
{
    /// <summary>
    /// A <see cref="SupportView"/> for rendering <see cref="Water"/> refractions or reflections
    /// </summary>
    public sealed class WaterView : SupportView
    {
        #region Variables
        private readonly CloneCamera _cloneCamera;
        #endregion

        #region Properties
        /// <summary>
        /// True if this is a <see cref="WaterViewSource.ReflectedView"/>, <c>false</c> if this is a <see cref="WaterViewSource.RefractedView"/>
        /// </summary>
        [Description("True if this is a reflection view, false if it is a refraction view"), Category("Behavior")]
        public bool Reflection { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new view for rendering <see cref="Water"/> refractions or reflections
        /// </summary>
        /// <param name="baseView">The <see cref="View"/> to base this support-view on</param>
        /// <param name="camera">The <see cref="CloneCamera"/> to look at the <see cref="Scene"/> with</param>
        /// <param name="reflection">True if this is a <see cref="WaterViewSource.ReflectedView"/>, <c>false</c> if this is a <see cref="WaterViewSource.RefractedView"/></param>
        private WaterView(View baseView, CloneCamera camera, bool reflection) :
            base(baseView, camera)
        {
            _cloneCamera = camera;
            Reflection = reflection;
        }
        #endregion

        #region Static access

        #region Refraction
        /// <summary>
        /// Creates a refraction of a <see cref="View"/> and adds it to <see cref="View.ChildViews"/>
        /// </summary>
        /// <param name="baseView">The <see cref="View"/> the refraction is based on</param>
        /// <param name="refractPlane">A plane alongside which the view shall be refracted</param>
        /// <param name="clipTolerance">How much to shift the clip plane away from the refract plane</param>
        /// <returns>The <see cref="WaterView"/> representing the refraction</returns>
        public static WaterView CreateRefraction(View baseView, DoublePlane refractPlane, float clipTolerance)
        {
            #region Sanity checks
            if (baseView == null) throw new ArgumentNullException(nameof(baseView));
            if (refractPlane == default(DoublePlane)) throw new ArgumentNullException(nameof(refractPlane));
            #endregion

            // Clone and modify the camera
            var newCamera = new CloneCamera(baseView.Camera)
            {
                Name = baseView.Camera.Name + " Refraction",
                ClipPlane = new DoublePlane(refractPlane.Point - refractPlane.Normal * clipTolerance, refractPlane.Normal)
            };

            // Create the new view, make sure the camera stays in sync, copy default properties
            var newView = new WaterView(baseView, newCamera, reflection: false)
            {
                Name = baseView.Name + " Refraction",
                BackgroundColor = baseView.BackgroundColor
            };

            baseView.ChildViews.Add(newView);
            return newView;
        }
        #endregion

        #region Reflection
        /// <summary>
        /// Creates a reflection of a <see cref="View"/> and adds it to <see cref="View.ChildViews"/>
        /// </summary>
        /// <param name="baseView">The <see cref="View"/> the reflection is based on</param>
        /// <param name="reflectPlane">A plane alongside which the view shall be reflected</param>
        /// <param name="clipTolerance">How much to shift the clip plane away from the reflect plane</param>
        /// <returns>The new view</returns>
        public static WaterView CreateReflection(View baseView, DoublePlane reflectPlane, float clipTolerance)
        {
            #region Sanity checks
            if (baseView == null) throw new ArgumentNullException(nameof(baseView));
            if (reflectPlane == default(DoublePlane)) throw new ArgumentNullException(nameof(reflectPlane));
            #endregion

            // Clone and modify the camera
            var newCamera = new ReflectCamera(baseView.Camera, reflectPlane)
            {
                Name = baseView.Camera.Name + " Reflection",
                ClipPlane = new DoublePlane(reflectPlane.Point - reflectPlane.Normal * clipTolerance, reflectPlane.Normal)
            };

            // Create the new view, make sure the camera stays in sync, copy default properties
            var newView = new WaterView(baseView, newCamera, reflection: true)
            {
                Name = baseView.Name + " Reflection",
                InvertCull = true,
                BackgroundColor = baseView.BackgroundColor
            };

            baseView.ChildViews.Add(newView);
            return newView;
        }
        #endregion

        #endregion

        //--------------------//

        #region Visibility check
        /// <inheritdoc/>
        protected override bool IsToRender(PositionableRenderable body)
        {
            #region Sanity checks
            if (body == null) throw new ArgumentNullException(nameof(body));
            #endregion

            // Filter bodies if this is a terrain-only reflection
            if (Reflection && Engine.Effects.WaterEffects <= WaterEffectsType.ReflectTerrain && !(body is Terrain))
                return false;

            // Perform the the more generic tests
            return base.IsToRender(body);
        }
        #endregion

        //--------------------//

        #region Render
        /// <inheritdoc/>
        internal override void Render()
        {
            // Keep the camera in sync with the base view
            _cloneCamera.ParentCamera = BaseView.Camera;
            Camera = _cloneCamera;

            base.Render();
        }
        #endregion
    }
}

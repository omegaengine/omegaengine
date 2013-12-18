/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Cameras;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// An object that can be <see cref="Render"/>ed by the <see cref="Engine"/>.
    /// </summary>
    public abstract class Renderable : EngineElement, IResetable
    {
        #region Variables
        private bool _preRenderDone;
        
        /// <summary>
        /// Occurs once per frame before rendering the entity.
        /// Will not be executed if the entity is excluded by a culling test.
        /// </summary>
        public event Action PreRender;

        protected virtual void OnPreRender()
        {
            if (!_preRenderDone && PreRender != null)
            {
                using (new ProfilerEvent("PreRender delegate"))
                    PreRender();
                _preRenderDone = true;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// How many times has this entity been rendered in this frame?
        /// </summary>
        /// <remarks>Used to debug culling methods</remarks>
        [Description("How many times has this entity been rendered in this frame?"), Category("Appearance")]
        public int RenderCount { get; private set; }

        #region Name
        /// <summary>
        /// Text value to make it easier to identify a particular render entity
        /// </summary>
        [Description("Text value to make it easier to identify a particular render entity"), Category("Design")]
        public string Name { get; set; }

        public override string ToString()
        {
            string value = GetType().Name;
            if (!string.IsNullOrEmpty(Name))
                value += ": " + Name;
            return value;
        }
        #endregion

        #region Flags
        /// <summary>
        /// Was this entity already disposed?
        /// </summary>
        [Browsable(false)]
        public bool Disposed { get; private set; }

        private bool _visible = true;
        /// <summary>
        /// Shall the entity be rendered?
        /// </summary>
        [DefaultValue(true), Description("Shall the entity be rendered?"), Category("Appearance")]
        public bool Visible { get { return _visible; } set { _visible = value; } }

        /// <summary>
        /// Shall this entity be drawn in wireframe-mode? (used for debugging)
        /// </summary>
        [DefaultValue(false), Description("Shall this entity be drawn in wireframe-mode? (used for debugging)"), Category("Appearance")]
        public bool Wireframe { get; set; }

        /// <summary>
        /// The maximum distance from which the entity is visible - 0 for infinite
        /// </summary>
        [DefaultValue(0f), Description("The maximum distance from which the entity is visible - 0 for infinite"), Category("Behavior")]
        public float VisibilityDistance { get; set; }

        /// <summary>
        /// The level of transparency from 0 (solid) to 255 (invisible),
        /// <see cref="OmegaEngine.EngineState.AlphaChannel"/>, <see cref="OmegaEngine.EngineState.BinaryAlphaChannel"/> or <see cref="OmegaEngine.EngineState.AdditivBlending"/>
        /// </summary>
        [DefaultValue(0), Description("The level of transparency from 0 (solid) to 255 (invisible), 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending"), Category("Appearance")]
        public int Alpha { get; set; }
        #endregion

        #endregion

        //--------------------//

        #region Reset
        /// <summary>
        /// Is to be called at the beginning of a frame.
        /// </summary>
        void IResetable.Reset()
        {
            _preRenderDone = false;
            RenderCount = 0;
        }
        #endregion

        #region Render
        /// <summary>
        /// Should be called before rendering anything - usually called by base constructor
        /// </summary>
        protected void PrepareRender()
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            OnPreRender();

            RenderCount++; // Culling-debug information
            Engine.QueueReset(this);

            Engine.State.FillMode = Wireframe ? FillMode.Wireframe : FillMode.Solid;
            Engine.State.AlphaBlend = Alpha;
        }

        /// <summary>
        /// To be called when this object ist to be rendered.
        /// </summary>
        /// <param name="camera">Supplies information for the view transformation.</param>
        /// <param name="lights">A delegate that will be called to get lighting information.</param>
        internal virtual void Render(Camera camera, GetLights lights)
        {
            PrepareRender();
        }
        #endregion
    }
}

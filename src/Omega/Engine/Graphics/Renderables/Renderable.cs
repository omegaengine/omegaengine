/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Common;
using SlimDX.Direct3D9;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// An object that can be <see cref="Render"/>ed by the <see cref="Engine"/>.
    /// </summary>
    public abstract class Renderable : IDisposable, IResetable
    {
        #region Variables
        /// <summary>
        /// The <see cref="Engine"/> reference to use for rendering operations
        /// </summary>
        protected readonly Engine Engine;

        #region PreRender
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

        /// <summary>
        /// Shall the entity be rendered?
        /// </summary>
        [DefaultValue(true), Description("Shall the entity be rendered?"), Category("Appearance")]
        public bool Visible { get; set; }

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
        /// <see cref="OmegaEngine.Engine.AlphaChannel"/>, <see cref="OmegaEngine.Engine.BinaryAlphaChannel"/> or <see cref="OmegaEngine.Engine.AdditivBlending"/>
        /// </summary>
        [DefaultValue(0), Description("The level of transparency from 0 (solid) to 255 (invisible), 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending"), Category("Appearance")]
        public int Alpha { get; set; }
        #endregion

        #endregion

        #region Constructor
        /// <summary>
        /// Creates a rendering entity
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
        protected Renderable(Engine engine)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            #endregion

            Visible = true;
            Engine = engine;
        }
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

            Engine.FillMode = Wireframe ? FillMode.Wireframe : FillMode.Solid;
            Engine.AlphaBlend = Alpha;
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

        //--------------------//

        #region Dispose
        /// <summary>
        /// Disposes any local unmanaged resources and releases any <see cref="Asset"/>s used.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        ~Renderable()
        {
            Dispose(false);
        }

        /// <summary>
        /// To be called by <see cref="IDisposable.Dispose"/> and the object destructor.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if called manually and not by the garbage collector.</param>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Only for debugging, not present in Release code")]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            { // This block will only be executed on Garbage Collection, not by manual disposal
                Log.Error("Forgot to call Dispose on " + this);
#if DEBUG
                throw new InvalidOperationException("Forgot to call Dispose on " + this);
#endif
            }

            Disposed = true;
        }
        #endregion
    }
}

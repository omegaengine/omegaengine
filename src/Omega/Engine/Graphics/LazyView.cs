/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using OmegaEngine.Graphics.Cameras;

namespace OmegaEngine.Graphics
{
    /// <summary>
    /// An <see cref="TextureView"/> that only handles <see cref="Render"/> when <see cref="SetDirty"/> has been called.
    /// </summary>
    /// <remarks>Useful for <see cref="Scene"/>s and <see cref="Camera"/>s that stay static for most of the time.</remarks>
    public sealed class LazyView : TextureView
    {
        #region Variables
        /// <summary>
        /// Does this <see cref="LazyView"/> need handle the next <see cref="Render"/> call?
        /// </summary>
        public bool Dirty { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new lazy view
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to be used for rendering</param>
        /// <param name="scene">The <see cref="Scene"/> to render</param>
        /// <param name="camera">The <see cref="Camera"/> to look at the <see cref="Scene"/> with</param>
        /// <param name="size">The size of screen area this view should fill (leave empty for fullscreen)</param>
        public LazyView(Engine engine, Scene scene, Camera camera, Size size) :
            base(engine, scene, camera, size)
        {
            Dirty = true;
        }
        #endregion

        //--------------------//

        #region Control
        /// <summary>
        /// Sets <see cref="Dirty"/> to <see langword="true"/>
        /// </summary>
        public void SetDirty()
        {
            Dirty = true;
        }
        #endregion

        #region Render
        /// <summary>
        /// Renders the view if <see cref="Dirty"/> is <see langword="true"/>.
        /// </summary>
        /// <remarks>At the end <see cref="Dirty"/> is set back to <see langword="false"/>.</remarks>
        internal override void Render()
        {
            if (!Dirty) return;

            base.Render();

            Dirty = false;
        }
        #endregion
    }
}

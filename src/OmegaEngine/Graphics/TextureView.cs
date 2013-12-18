/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using OmegaEngine.Graphics.Cameras;

namespace OmegaEngine.Graphics
{
    /// <summary>
    /// A special kind of <see cref="View"/> that directs its output into a texture <see cref="RenderTarget"/> instead of printing straight to the screen.
    /// </summary>
    /// <remarks>These <see cref="View"/>s usually provide helper data (shadows, reflections) for the main <see cref="View"/>s.
    /// They are then referenced in <see cref="View.ChildViews"/>.</remarks>
    public class TextureView : View
    {
        #region Properties
        /// <summary>
        /// Not applicable to <see cref="TextureView"/>
        /// </summary>
        [Browsable(false)]
        public override sealed int FullAlpha { get { return 0; } set { } }

        /// <summary>
        /// Does this <see cref="View"/> render to a texture <see cref="RenderTarget"/>? <see langword="true"/> since this is a <see cref="TextureView"/>.
        /// </summary>
        protected override bool TextureRenderTarget { get { return true; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new view for rendering to a texture
        /// </summary>
        /// <param name="scene">The <see cref="Scene"/> to render</param>
        /// <param name="camera">The <see cref="Camera"/> to look at the <see cref="Scene"/> with</param>
        /// <param name="size">The size of screen area this view should fill (leave empty for fullscreen)</param>
        public TextureView(Scene scene, Camera camera, Size size) :
            base(scene, camera, new Rectangle(new Point(), size))
        {}
        #endregion

        //--------------------//

        #region Render-to-texture
        /// <summary>
        /// The texture this view renders to. May change, do not store externally!
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Requesting a render target has a considerable performance cost.")]
        public RenderTarget GetRenderTarget()
        {
            PrepareRenderTarget();
            return RenderTarget;
        }
        #endregion

        #region Apply post-screen shaders
        /// <inheritdoc />
        protected override void ApplyPostShaders(bool sceneOnBackBuffer)
        {
            // Note: Doesn't call base methods

            #region Sanity checks
            Debug.Assert(!sceneOnBackBuffer, "Scene should never be on back-buffer with external render target");
            #endregion

            foreach (var shader in PostShaders)
            {
                // Update the RenderTarget texture to incorporate the PostShader effect
                if (shader.Enabled) ShaderToRenderTarget(shader);
            }
        }
        #endregion

        //--------------------//

        #region Engine
        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            base.OnEngineSet();
            PrepareRenderTarget();
        }
        #endregion
    }
}

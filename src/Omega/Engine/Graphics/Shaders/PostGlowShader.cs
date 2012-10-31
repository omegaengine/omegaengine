/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using Common;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    ///A post-screen shader that adds a bloom-like glow-effect around objects in the scene.
    /// </summary>
    public class PostGlowShader : PostBlurShader
    {
        #region Variables
        private float _glowStrength = 1;

        private readonly EffectHandle _glowStrengthHandle;

        private readonly TextureView _glowView;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public new static Version MinShaderModel { get { return new Version(2, 0); } }

        /// <summary>
        /// Does this post-screen shader use overlay rendering instead of a scene map?
        /// </summary>
        [Description("Does this post-screen shader use overlay rendering instead of a scene map?")]
        public override sealed bool OverlayRendering { get { return true; } }

        /// <summary>
        /// A factor by which the blurred glow color is multiplied - values between 0 and 100
        /// </summary>
        [DefaultValue(1f), Description("A factor by which the blurred glow color is multiplied - values between 0 and 100")]
        public float GlowStrength
        {
            get { return _glowStrength; }
            set
            {
                if (Disposed) return;
                if (value < 0 || value > 100) throw new ArgumentOutOfRangeException("value");
                UpdateHelper.Do(ref _glowStrength, value, () => Effect.SetValue(_glowStrengthHandle, value));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the shader
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <param name="glowView">A render target storing the glow map of the current view</param>
        /// <param name="blurStrength">How strongly to blur the glow map - values between 0 and 10</param>
        /// <param name="glowStrength">A factor by which the blurred glow color is multiplied - values between 0 and 100</param>
        /// <exception cref="NotSupportedException">Thrown if the graphics card does not support this shader</exception>
        public PostGlowShader(Engine engine, TextureView glowView, float blurStrength, float glowStrength) : base(engine)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (glowView == null) throw new ArgumentNullException("glowView");
            if (MinShaderModel > engine.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            _glowView = glowView;

            // Output is layered on top of existing scene via additive alpha-blending
            Effect.Technique = "Glow";

            // Get handles to shader parameters for quick access
            _glowStrengthHandle = Effect.GetParameter(null, "GlowStrength");

            BlurStrength = blurStrength;
            GlowStrength = glowStrength;
        }
        #endregion

        //--------------------//

        #region Passes
        /// <summary>
        /// Runs the actual shader passes
        /// </summary>
        /// <param name="render">The render delegate (is called once for every shader pass)</param>
        /// <param name="sceneSize">The size of the scene on the screen - leave empty for fullscreen</param>
        /// <param name="sceneMap">Should be <see langword="null"/> because a glow map is used instead</param>
        protected override void RunPasses(Action render, Size sceneSize, RenderTarget sceneMap)
        {
            // Pass the glow map instead of the scene map to the blurring filter
            base.RunPasses(render, sceneSize, _glowView.GetRenderTarget());
        }
        #endregion
    }
}

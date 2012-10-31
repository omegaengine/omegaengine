/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Common;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A shader that is applied to the complete scene after rendering.
    /// </summary>
    /// <seealso cref="View.PostShaders"/>
    public abstract class PostShader : Shader
    {
        #region Properties
        /// <summary>
        /// Does this post-screen shader use overlay rendering instead of a scene map?
        /// </summary>
        [Description("Does this post-screen shader use overlay rendering instead of a scene map?")]
        public virtual bool OverlayRendering { get { return false; } }

        /// <summary>
        /// Shall this post-screen effect be applied?
        /// </summary>
        [DefaultValue(true), Description("Shall this post-screen effect be applied?")]
        public bool Enabled { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Loads a post-screen shader from a file
        /// </summary>
        /// <param name="engine">The <see cref="OmegaEngine.Engine"/> reference to use for rendering operations</param>
        /// <param name="path">The shader file path relative to the shader directory or as an absolute path</param>
        protected PostShader(Engine engine, string path) : base(engine, path)
        {
            Enabled = true;
        }
        #endregion

        //--------------------//

        #region Passes
        /// <summary>
        /// Runs the actual shader passes
        /// </summary>
        /// <param name="render">The render delegate (is called once for every shader pass)</param>
        /// <param name="sceneSize">The size of the scene on the screen - leave empty for fullscreen</param>
        /// <param name="sceneMap">A texture containing the rendered scene, <see langword="null"/> if the shader doesn't need it</param>
        protected virtual void RunPasses(Action render, Size sceneSize, RenderTarget sceneMap)
        {
            if (render == null) throw new ArgumentNullException("render");

            int passCount = Effect.Begin(FX.None);
            IList<SasScriptCommand> techniqueScript;
            if (Techniques.TryGetValue(Effect.Technique, out techniqueScript))
                ExecuteScript(techniqueScript, render, sceneSize, sceneMap);
            else
            {
                for (int i = 0; i < passCount; i++)
                {
                    // ReSharper disable AccessToModifiedClosure
                    using (new ProfilerEvent(() => "Pass " + i))
                    {
                        Effect.BeginPass(i);
                        render();
                        Effect.EndPass();
                    }
                    // ReSharper restore AccessToModifiedClosure
                }
            }

            Effect.End();
        }
        #endregion

        #region Apply
        /// <summary>
        /// Applies a post-screen shader to a scene
        /// </summary>
        /// <param name="render">The delegate to call back for rendering the output</param>
        /// <param name="sceneSize">The size of the scene on the screen - leave empty for fullscreen</param>
        /// <param name="sceneMap">A texture containing the rendered scene, <see langword="null"/> if the shader doesn't need it</param>
        internal virtual void Apply(Action render, Size sceneSize, RenderTarget sceneMap)
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            if (render == null) throw new ArgumentNullException("render");
            #endregion

            #region Values
            foreach (var info in ParameterInfos)
            {
                switch (info.Type)
                {
                    case ParameterType.Float:
                        switch (info.SemanticID)
                        {
                                #region Viewport
                            case SemanticID.ViewportPixelSize:
                                Size viewportSize = (sceneSize == Size.Empty) ? new Size(Engine.RenderSize.Width, Engine.RenderSize.Height) : sceneSize;
                                Effect.SetValue(info.Handle, new float[] {viewportSize.Width, viewportSize.Height});
                                break;
                                #endregion

                                #region Timer
                            case SemanticID.Time:
                                Effect.SetValue(info.Handle, (float)Engine.TotalTime);
                                break;
                            case SemanticID.ElapsedTime:
                                Effect.SetValue(info.Handle, (float)Engine.ThisFrameTime);
                                break;
                                #endregion
                        }
                        break;

                    case ParameterType.Texture:
                        switch (info.SemanticID)
                        {
                                #region Render target
                            case SemanticID.RenderColorTarget:
                                Effect.SetTexture(info.Handle, sceneMap);
                                break;
                            case SemanticID.RenderDepthStencilTarget:
                                // ToDo: Implement
                                Effect.SetTexture(info.Handle, null);
                                break;
                                #endregion
                        }
                        break;
                }
            }
            #endregion

            RunPasses(render, sceneSize, sceneMap);
        }
        #endregion
    }
}

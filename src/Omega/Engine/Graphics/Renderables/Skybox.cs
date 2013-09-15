/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using SlimDX;
using OmegaEngine.Graphics.Cameras;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// Provides a backgound for a <see cref="Scene"/> that "follows" the <see cref="Camera.Position"/>, creating the illusion of infinite distance.
    /// </summary>
    /// <seealso cref="Scene.Skybox"/>
    public abstract class Skybox : Renderable
    {
        #region Variables
        [SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly", Justification = "It is deliberate that the array size cannot be changed while the elements can be modified")]
        protected readonly ITextureProvider[] Textures = new ITextureProvider[6];
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new skybox using texture-files
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
        /// <param name="textures">An array of the 6 textures to be uses (right, left, top, bottom, front, back)</param>
        /// <exception cref="ArgumentException">Thrown if there are not exactly 6 textures</exception>
        protected Skybox(Engine engine, ITextureProvider[] textures) : base(engine)
        {
            #region Sanity checks
            if (textures == null) throw new ArgumentNullException("textures");
            if (textures.Length != 6) throw new ArgumentException(string.Format(Resources.WrongTexArrayLength, "6"), "textures");
            #endregion

            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] != null)
                {
                    Textures[i] = textures[i];
                    Textures[i].HoldReference();
                }
            }
        }

        /// <inheritdoc />
        internal override void Render(Camera camera, GetLights lights)
        {
            base.Render(camera, lights);

            // Scale by factor 3, which should be safe within a simple projection matrix (near=1 and far=10)
            Engine.State.WorldTransform = Matrix.Scaling(3, 3, 3);

            // Deactivate lighting
            Engine.State.FfpLighting = false;
        }
        #endregion

        //--------------------//

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (Disposed || Engine == null || Engine.Disposed) return; // Don't try to dispose more than once

            try
            {
                if (disposing)
                {
                    // This block will only be executed on manual disposal, not by Garbage Collection
                    for (int i = 0; i < Textures.Length; i++)
                    {
                        if (Textures[i] != null)
                        {
                            Textures[i].ReleaseReference();
                            Textures[i] = null;
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}

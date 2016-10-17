/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// A skybox with multiple layers for cloud and moon animation.
    /// </summary>
    public class AdvancedSkybox : Skybox
    {
        #region Properties
        /// <summary>
        /// How fast the clouds should move.
        /// </summary>
        public int CloudSpeed { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new skybox using texture-files
        /// </summary>
        /// <param name="textures">An array of the 6 textures to be uses (front, right, back, left, top, bottom)</param>
        /// <exception cref="ArgumentException">There are not exactly 6 textures.</exception>
        protected AdvancedSkybox(ITextureProvider[] textures) : base(textures)
        {
            #region Sanity checks
            if (textures == null) throw new ArgumentNullException(nameof(textures));
            if (textures.Length != 6)
                throw new ArgumentException(string.Format(Resources.WrongTexArrayLength, "6"), nameof(textures));
            #endregion

            // ToDo: Implement
            throw new NotImplementedException();
        }
        #endregion

        #region Static access
        /// <summary>
        /// Creates a new skybox using a cached <see cref="XTexture"/>s (loading new ones if they aren't cached).
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> providing the cache and rendering capabilities.</param>
        /// <param name="rt">The ID of the "right" texture.</param>
        /// <param name="lf">The ID of the "left" texture</param>
        /// <param name="up">The ID of the "up" texture.</param>
        /// <param name="dn">The ID of the "down" texture.</param>
        /// <param name="ft">The ID of the "front" texture.</param>
        /// <param name="bk">The ID of the "back" texture.</param>
        /// <returns>The skybox that was created.</returns>
        /// <exception cref="FileNotFoundException">On of the specified texture files could not be found.</exception>
        /// <exception cref="IOException">There was an error reading one of the texture files.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to one of the texture files is not permitted.</exception>
        /// <exception cref="InvalidDataException">One of the texture files does not contain a valid texture.</exception>
        public static AdvancedSkybox FromAssets(Engine engine, string rt, string lf, string up, string dn, string ft, string bk)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            #endregion

            var textures = new[]
            {
                XTexture.Get(engine, rt), XTexture.Get(engine, lf), XTexture.Get(engine, up),
                XTexture.Get(engine, dn), XTexture.Get(engine, ft), XTexture.Get(engine, bk)
            };
            return new AdvancedSkybox(textures);
        }
        #endregion

        //--------------------//

        #region Render
        /// <inheritdoc/>
        internal override void Render(Camera camera, GetLights getLights = null)
        {
            base.Render(camera, getLights);

            // ToDo: Implement
            throw new NotImplementedException();
        }
        #endregion

        #region Access
        /// <summary>
        /// Sets the time of the day.
        /// </summary>
        /// <param name="daytime">The time in hours (Night(0), Morning(6), Midday(12), Evening(18)).</param>
        /// <returns>The resulting color used for lighting the inner Skybox.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Method not implemented")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method not implemented")]
        public Color SetDaytime(float daytime)
        {
            // ToDo: Implement
            throw new NotImplementedException();
        }
        #endregion
    }
}

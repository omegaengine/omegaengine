/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using Common.Storage;
using Common.Utils;
using SlimDX;
using OmegaEngine.Graphics.Shaders;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// A set of information about a particle system as a whole
    /// </summary>
    [XmlRoot("GpuParticlePreset")] // Supress XMLSchema declarations (no inheritance used for properties)
    public class GpuParticlePreset : ICloneable
    {
        #region Variables
        private float _spawnRadius = 1, _systemHeight = 80;
        private Vector3 _movement = new Vector3(0, 0.48f, 0);
        private float _particleSpread = 20, _particleSize = 7.8f, _particleShape = 0.38f;

        private string _particleTexture = "Flame.tga";

        /// <summary>
        /// Flag indicating that <see cref="ParticleShader.ParticleTexture"/> has changed and needs to be reloaded
        /// </summary>
        internal bool TextureDirty;
        #endregion

        #region Properties
        /// <summary>
        /// The largest distance from the emitter at which particle shall be spawned
        /// </summary>
        [DefaultValue(1f), Description("The largest distance from the emitter at which particle shall be spawned")]
        public float SpawnRadius { get { return _spawnRadius; } set { _spawnRadius = value; } }

        /// <summary>
        /// The largest distance from the emitter particles can travel before dying
        /// </summary>
        [DefaultValue(80f), Description("The largest distance from the emitter particles can travel before dying")]
        public float SystemHeight { get { return _systemHeight; } set { _systemHeight = value; } }

        /// <summary>
        /// The direction and speed with which the particles move
        /// </summary>
        [DefaultValue(typeof(Vector3), "0; 0.48; 0"), Description("The direction and speed with which the particles move")]
        public Vector3 Movement { get { return _movement; } set { _movement = value; } }

        /// <summary>
        /// How to spread the particles
        /// </summary>
        [DefaultValue(20f), Description("How to spread the particles")]
        public float ParticleSpread { get { return _particleSpread; } set { _particleSpread = value; } }

        /// <summary>
        /// The size of the particles
        /// </summary>
        [DefaultValue(7.8f), Description("The size of the particles")]
        public float ParticleSize { get { return _particleSize; } set { _particleSize = value; } }

        /// <summary>
        /// The shape of the particles
        /// </summary>
        [DefaultValue(0.38f), Description("The shape of the particles")]
        public float ParticleShape { get { return _particleShape; } set { _particleShape = value; } }

        /// <summary>
        /// The ID of the texture to use for color lookup
        /// </summary>
        [DefaultValue("Flame.tga"), Description("The ID of the texture to use for color lookup")]
        public string ParticleTexture { get { return _particleTexture; } set { value.To(ref _particleTexture, ref TextureDirty); } }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads a preset from an XML file via the <see cref="ContentManager"/>.
        /// </summary>
        /// <param name="id">The ID of the XML file to load</param>
        /// <returns>The loaded preset</returns>
        /// <exception cref="FileNotFoundException">Thrown if the specified file could not be found.</exception>
        /// <exception cref="IOException">Thrown if there was an error reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurred while deserializing the XML data.</exception>
        public static GpuParticlePreset FromContent(string id)
        {
            using (var stream = ContentManager.GetFileStream("Graphics/GpuParticleSystem", id))
                return XmlStorage.LoadXml<GpuParticlePreset>(stream);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of the this particle system preset
        /// </summary>
        /// <returns>The cloned preset</returns>
        public GpuParticlePreset Clone()
        {
            var newPreset = (GpuParticlePreset)MemberwiseClone();
            return newPreset;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}

/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using NanoByte.Common.Storage.SlimDX;
using NanoByte.Common.Utils;
using SlimDX;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// A set of information about a particle system as a whole
    /// </summary>
    [XmlRoot("CpuParticlePreset")] // Supress XMLSchema declarations (no inheritance used for properties)
    public class CpuParticlePreset : ICloneable
    {
        #region Variables
        private int _maxParticles = 512;

        private float _speed = 1;
        private float _emitterSuctionRange = 32768;

        private string _particleTexture1, _particleTexture2;

        /// <summary>
        /// Flag indicating that <see cref="Particle1Texture"/> and/or <see cref="Particle2Texture"/> have changed and need to be reloaded
        /// </summary>
        internal bool TexturesDirty;
        #endregion

        #region Properties

        #region Particle parameters
        /// <summary>
        /// The lower values of the range of parameters used to spawn new particles
        /// </summary>
        [Browsable(false)]
        public CpuParticleParameters LowerParameters1 { get; set; }

        /// <summary>
        /// The upper values of the range of parameters used to spawn new particles
        /// </summary>
        [Browsable(false)]
        public CpuParticleParameters UpperParameters1 { get; set; }

        /// <summary>
        /// <see langword="true"/> <see cref="CpuParticleParameters.LifeTime"/> is set to <see cref="CpuParticleParameters.InfiniteFlag"/> for <see cref="LowerParameters1"/> or <see cref="UpperParameters1"/>.
        /// </summary>
        [Browsable(false)]
        public bool InfiniteLifetime1
        {
            get
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                return LowerParameters1.LifeTime == CpuParticleParameters.InfiniteFlag || UpperParameters1.LifeTime == CpuParticleParameters.InfiniteFlag;
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }
        }

        /// <summary>
        /// The lower values of the range of parameters used to start particles' "second life"
        /// </summary>
        [Browsable(false)]
        public CpuParticleParameters LowerParameters2 { get; set; }

        /// <summary>
        /// The upper values of the range of parameters used to start particles' "second life"
        /// </summary>
        [Browsable(false)]
        public CpuParticleParameters UpperParameters2 { get; set; }

        /// <summary>
        /// <see langword="true"/> <see cref="CpuParticleParameters.LifeTime"/> is set to <see cref="CpuParticleParameters.InfiniteFlag"/> for <see cref="LowerParameters2"/> or <see cref="UpperParameters2"/>.
        /// </summary>
        [Browsable(false)]
        public bool InfiniteLifetime2
        {
            get
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                return LowerParameters2.LifeTime == CpuParticleParameters.InfiniteFlag || UpperParameters2.LifeTime == CpuParticleParameters.InfiniteFlag;
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }
        }
        #endregion

        #region Spawn
        /// <summary>
        /// How many new particles shall be spawned per second
        /// </summary>
        [DefaultValue(0f), Category("Spawn"), Description("How many new particles shall be spawned per second")]
        public float SpawnRate { get; set; }

        /// <summary>
        /// The largest distance from the emitter at which particle shall be spawned
        /// </summary>
        [DefaultValue(0f), Category("Spawn"), Description("The largest distance from the emitter at which particle shall be spawned")]
        public float SpawnRadius { get; set; }

        /// <summary>
        /// The maximum number particles in existance at any one point in time
        /// </summary>
        [DefaultValue(512), Category("Spawn"), Description("The maximum number particles in existance at any one point in time")]
        public int MaxParticles { get { return _maxParticles; } set { _maxParticles = value; } }
        #endregion

        #region Acceleration
        /// <summary>
        /// How far the emitter's repelling force can reach
        /// </summary>
        [DefaultValue(0f), Category("Acceleration"), Description("How far the emitter's repelling force can reach")]
        public float EmitterRepelRange { get; set; }

        /// <summary>
        /// How fast particles will be pushed away from the emitter's centre
        /// </summary>
        [DefaultValue(0f), Category("Acceleration"), Description("How fast particles will be pushed away from the emitter's centre")]
        public float EmitterRepelSpeed { get; set; }

        /// <summary>
        /// From where the emitter suction force starts to act
        /// </summary>
        [DefaultValue(32768f), Category("Acceleration"), Description("When the emitter suction force starts")]
        public float EmitterSuctionRange { get { return _emitterSuctionRange; } set { _emitterSuctionRange = value; } }

        /// <summary>
        /// How fast particles will be sucked back to the emitter's centre
        /// </summary>
        [DefaultValue(0f), Category("Acceleration"), Description("How fast particles will be sucked back to the emitter's centre")]
        public float EmitterSuctionSpeed { get; set; }

        /// <summary>
        /// A permanent acceleration force applied to all particles
        /// </summary>
        [Category("Acceleration"), Description("A permanent acceleration force applied to all particles")]
        public Vector3 Gravity { get; set; }

        /// <summary>
        /// Randomly accelerate particles with up to the specified speed
        /// </summary>
        [DefaultValue(0f), Category("Acceleration"), Description("Randomly accelerate particles with up to the specified speed")]
        public float RandomAcceleration { get; set; }
        #endregion

        #region General
        /// <summary>
        /// A factor by which all elapsed times are multiplied
        /// </summary>
        [DefaultValue(1f), Category("General"), Description("A factor by which all elapsed times are multiplied")]
        public float Speed { get { return _speed; } set { _speed = value; } }

        /// <summary>
        /// How many seconds to "fast forward" the particle system before it's render the first time
        /// </summary>
        [DefaultValue(0f), Category("General"), Description("How many seconds to \"fast forward\" the particle system before it's render the first time")]
        public float WarmupTime { get; set; }
        #endregion

        #region Render
        /// <summary>
        /// The ID of the texture to place on the particles
        /// </summary>
        [DefaultValue(""), Category("Render"), Description("The ID of the texture to place on the particles")]
        public string Particle1Texture { get { return _particleTexture1; } set { value.To(ref _particleTexture1, ref TexturesDirty); } }

        /// <summary>
        /// The level of transparency from 0 (solid) to 255 (invisible),
        /// <see cref="EngineState.AlphaChannel"/>, <see cref="EngineState.BinaryAlphaChannel"/> or <see cref="EngineState.AdditivBlending"/>
        /// </summary>
        [DefaultValue(0), Category("Render"), Description("The level of transparency from 0 (solid) to 255 (invisible), 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending")]
        public int Particle1Alpha { get; set; }

        /// <summary>
        /// The ID of the texture to place on the particles during their "second life"
        /// </summary>
        [DefaultValue(""), Category("Render"), Description("The ID of the texture to place on the particles during their \"second life\"")]
        public string Particle2Texture { get { return _particleTexture2; } set { value.To(ref _particleTexture2, ref TexturesDirty); } }

        /// <summary>
        /// The level of transparency from 0 (solid) to 255 (invisible) for particles' "second life",
        /// <see cref="EngineState.AlphaChannel"/>, <see cref="EngineState.BinaryAlphaChannel"/> or <see cref="EngineState.AdditivBlending"/>
        /// </summary>
        [DefaultValue(0), Category("Render"), Description("The level of transparency from 0 (solid) to 255 (invisible) for particles' \"second life\", 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending")]
        public int Particle2Alpha { get; set; }
        #endregion

        #endregion

        #region Constructor
        public CpuParticlePreset()
        {
            LowerParameters1 = new CpuParticleParameters {LifeTime = 2, Size = 10};
            UpperParameters1 = new CpuParticleParameters {LifeTime = 2, Size = 10};
            LowerParameters2 = new CpuParticleParameters();
            UpperParameters2 = new CpuParticleParameters();

            // Pre-initialize with some useful default values (not the "official" default values for serialization)
            Particle1Texture = "fire.png";
            Particle1Alpha = 257;
            SpawnRate = 10;
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads a preset from an XML file via the <see cref="ContentManager"/>.
        /// </summary>
        /// <param name="id">The ID of the XML file to load</param>
        /// <returns>The loaded preset</returns>
        /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
        /// <exception cref="IOException">There was an error reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">A problem occurred while deserializing the XML data.</exception>
        public static CpuParticlePreset FromContent(string id)
        {
            using (var stream = ContentManager.GetFileStream("Graphics/CpuParticleSystem", id))
                return XmlStorage.LoadXml<CpuParticlePreset>(stream);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of the this particle system preset
        /// </summary>
        /// <returns>The cloned preset</returns>
        public CpuParticlePreset Clone()
        {
            var newPreset = (CpuParticlePreset)MemberwiseClone();
            newPreset.LowerParameters1 = LowerParameters1.CloneParameters();
            newPreset.UpperParameters1 = UpperParameters1.CloneParameters();
            newPreset.LowerParameters2 = LowerParameters2.CloneParameters();
            newPreset.UpperParameters2 = UpperParameters2.CloneParameters();
            return newPreset;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}

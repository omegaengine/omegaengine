/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Common.Utils;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A shader that blends multiple textures together
    /// </summary>
    /// <seealso cref="Terrain"/>
    public class TerrainShader : LightingShader
    {
        #region Variables
        private readonly bool _lighting;
        private float _blendDistance = 400, _blendWidth = 700;

        private readonly EffectHandle _simple14, _simple20;
        private readonly EffectHandle _light14, _light20, _light2A, _light2B;
        private readonly EffectHandle _simpleBlack, _lightBlack;
        private readonly EffectHandle _blendDistanceHandle, _blendWidthHandle;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(1, 4); } }

        /// <summary>
        /// The distance at which to show the pure near texture
        /// </summary>
        [DefaultValue(400f), Description("The distance at which to show the pure near texture")]
        public float BlendDistance
        {
            get { return _blendDistance; }
            set
            {
                if (Disposed) return;
                value.To(ref _blendDistance, () => Effect.SetValue(_blendDistanceHandle, value));
            }
        }

        /// <summary>
        /// The distance from <see cref="BlendDistance"/> where to show the pure far texture
        /// </summary>
        [DefaultValue(700f), Description("The distance from BlendDistance where to show the pure far texture")]
        public float BlendWidth
        {
            get { return _blendWidth; }
            set
            {
                if (Disposed) return;
                value.To(ref _blendWidth, () => Effect.SetValue(_blendWidthHandle, value));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a specialized instance of the shader
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <param name="lighting">Shall this shader apply lighting to the terrain?</param>
        /// <param name="controllers">A set of int arrays that control the counters</param>
        /// <exception cref="NotSupportedException">Thrown if the graphics card does not support this shader</exception>
        public TerrainShader(Engine engine, bool lighting, IDictionary<string, IEnumerable<int>> controllers)
            : base(engine, DynamicShader.FromContent(engine, "Terrain.fxd", lighting, controllers))
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (controllers == null) throw new ArgumentNullException("controllers");
            if (MinShaderModel > engine.MaxShaderModel)
                throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            #region Technique handles
            _simple14 = "Simple14";
            _simple20 = "Simple20";
            _light14 = "Light14";
            _light20 = "Light20";
            _light2A = "Light2a";
            _light2B = "Light2b";
            _simpleBlack = "SimpleBlack";
            _lightBlack = "LightBlack";
            #endregion

            _lighting = lighting;

            // Get handles to shader parameters for quick access
            _blendDistanceHandle = Effect.GetParameter(null, "BlendDistance");
            _blendWidthHandle = Effect.GetParameter(null, "BlendWidth");
        }
        #endregion

        //--------------------//

        #region Apply
        /// <summary>
        /// Applies the shader to the content in the render delegate.
        /// </summary>
        /// <param name="render">The render delegate (is called once for every shader pass).</param>
        /// <param name="material">The material to be used by this shader; <see langword="null"/> for device texture.</param>
        /// <param name="camera">The camera for transformation information.</param>
        /// <param name="lights">An array of all lights this shader should consider. Mustn't be <see langword="null"/>!</param>
        public override void Apply(Action render, XMaterial material, Camera camera, LightSource[] lights)
        {
            #region Sanity checks
            if (render == null) throw new ArgumentNullException("render");
            if (camera == null) throw new ArgumentNullException("camera");
            if (lights == null) throw new ArgumentNullException("lights");
            #endregion

            #region Auto-select technique
            if (lights.Length == 0 && _lighting)
                Effect.Technique = _lighting ? _lightBlack : _simpleBlack;
            else
            {
                if (Engine.DoubleSampling && _lighting)
                {
                    if (Engine.MaxShaderModel >= new Version(2, 0, 2))
                        Effect.Technique = _light2B;
                    else if (Engine.MaxShaderModel == new Version(2, 0, 1))
                        Effect.Technique = _light2A;
                }
                else if (Engine.MaxShaderModel >= new Version(2, 0))
                    Effect.Technique = _lighting ? _light20 : _simple20;
                else
                    Effect.Technique = _lighting ? _light14 : _simple14;
            }
            #endregion

            base.Apply(render, material, camera, _lighting ? lights : null);
        }
        #endregion
    }
}

/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Cameras;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A general-purpose surface shader with optional support for normal and specular maps
    /// </summary>
    /// <seealso cref="Engine.DefaultShader"/>
    public class GeneralShader : LightingShader
    {
        #region Variables
        private readonly EffectHandle
            _coloredPerVertex, _coloredPerPixel, _coloredEmissiveOnly,
            _texturedPerVertex, _texturedPerPixel, _texturedPerPixelNormalMap,
            _texturedPerPixelSpecularMap, _texturedPerPixelNormalSpecularMap,
            _texturedPerPixelEmissiveMap, _texturedPerPixelNormalEmissiveMap,
            _texturedPerPixelNormalSpecularEmissiveMap, _texturedEmissiveOnly, _texturedEmissiveMapOnly;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(1, 1); } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the shader
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <exception cref="NotSupportedException">Thrown if the graphics card does not support this shader</exception>
        public GeneralShader(Engine engine) : base(engine, "General.fxo")
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (MinShaderModel > engine.MaxShaderModel)
                throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            #region Technique handles
            _coloredPerVertex = "ColoredPerVertex";
            _coloredPerPixel = "ColoredPerPixel";
            _coloredEmissiveOnly = "ColoredEmissiveOnly";
            _texturedPerVertex = "TexturedPerVertex";
            _texturedPerPixel = "TexturedPerPixel";
            _texturedPerPixelNormalMap = "TexturedPerPixelNormalMap";
            _texturedPerPixelSpecularMap = "TexturedPerPixelSpecularMap";
            _texturedPerPixelNormalSpecularMap = "TexturedPerPixelNormalSpecularMap";
            _texturedPerPixelEmissiveMap = "TexturedPerPixelEmissiveMap";
            _texturedPerPixelNormalEmissiveMap = "TexturedPerPixelNormalEmissiveMap";
            _texturedPerPixelNormalSpecularEmissiveMap = "TexturedPerPixelNormalSpecularEmissiveMap";
            _texturedEmissiveOnly = "TexturedEmissiveOnly";
            _texturedEmissiveMapOnly = "TexturedEmissiveMapOnly";
            #endregion
        }
        #endregion

        //--------------------//

        #region Apply
        /// <inheritdoc />
        internal override void Apply(Action render, XMaterial material, Camera camera, LightSource[] lights)
        {
            #region Sanity checks
            if (render == null) throw new ArgumentNullException("render");
            if (camera == null) throw new ArgumentNullException("camera");
            if (lights == null) throw new ArgumentNullException("lights");
            #endregion

            #region Auto-select technique
            if (lights.Length == 0)
            { // Only emissive lighting
                if (Engine.PerPixelLighting && material.EmissiveMap != null)
                    Effect.Technique = _texturedEmissiveMapOnly;
                else
                    Effect.Technique = (material.DiffuseMaps[0] == null) ? _coloredEmissiveOnly : _texturedEmissiveOnly;
            }
            else
            {
                if (Engine.PerPixelLighting)
                { // Normal per-pixel lighting
                    if (material.DiffuseMaps[0] == null)
                        Effect.Technique = _coloredPerPixel;
                    else
                    {
                        #region Flags
                        bool normal = material.NormalMap != null && Engine.NormalMapping;
                        bool specular = material.SpecularMap != null;
                        bool emissive = material.EmissiveMap != null;
                        #endregion

                        if (normal && specular && emissive) Effect.Technique = _texturedPerPixelNormalSpecularEmissiveMap;
                        else if (normal && emissive) Effect.Technique = _texturedPerPixelNormalEmissiveMap;
                        else if (emissive) Effect.Technique = _texturedPerPixelEmissiveMap;
                        else if (normal && specular) Effect.Technique = _texturedPerPixelNormalSpecularMap;
                        else if (specular) Effect.Technique = _texturedPerPixelSpecularMap;
                        else if (normal) Effect.Technique = _texturedPerPixelNormalMap;
                        else Effect.Technique = _texturedPerPixel;
                    }
                }
                else
                { // Normal per-vertex lighting
                    Effect.Technique = (material.DiffuseMaps[0] == null) ? _coloredPerVertex : _texturedPerVertex;
                }
            }
            #endregion

            base.Apply(render, material, camera, lights);
        }
        #endregion
    }
}

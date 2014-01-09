/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using Common.Utils;
using Common.Values;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using OmegaEngine.Graphics.Cameras;

namespace OmegaEngine.Graphics
{
    /// <summary>
    /// A light source that has a fixed position and shines uniformly in all directions.
    /// </summary>
    public sealed class PointLight : LightSource, IPositionableOffset
    {
        #region Properties
        /// <summary>
        /// Shall this light source be converted to a pseudo-directional source for each individual <see cref="PositionableRenderable"/> before passing it to shaders?
        /// </summary>
        [Description("Shall this light source be converted to a pseudo-directional source for each individual PositionableRenderable before passing it to shaders?"), Category("Behavior")]
        public bool DirectionalForShader { get; set; }

        private DoubleVector3 _position;

        /// <summary>
        /// The position of the light source
        /// </summary>
        [Description("The position of the light source"), Category("Layout")]
        public DoubleVector3 Position { get { return _position; } set { value.To(ref _position, ref _effectivePositionDirty); } }

        private DoubleVector3 _positionOffset;

        /// <summary>
        /// A value to be added to <see cref="Position"/> in order gain <see cref="IPositionableOffset.EffectivePosition"/> - auto-updated by <see cref="View.Render"/> to the negative <see cref="Camera.Position"/>
        /// </summary>
        DoubleVector3 IPositionableOffset.Offset { get { return _positionOffset; } set { value.To(ref _positionOffset, ref _effectivePositionDirty); } }

        private bool _effectivePositionDirty;
        private Vector3 _effectivePosition;

        /// <summary>
        /// The body's position in render space, based on <see cref="Position"/>
        /// </summary>
        /// <remarks>Constantly changes based on the values set for <see cref="IPositionableOffset.Offset"/></remarks>
        Vector3 IPositionableOffset.EffectivePosition
        {
            get
            {
                if (_effectivePositionDirty)
                {
                    _effectivePosition = _position.ApplyOffset(((IPositionableOffset)this).Offset);
                    _effectivePositionDirty = false;
                }
                return _effectivePosition;
            }
        }

        /// <summary>
        /// The maximum distance at which the light source has an effect.
        /// </summary>
        [Description("The maximum distance at which the light source has an effect."), Category("Behavior")]
        public float Range { get; set; }

        /// <summary>
        /// Factors describing the attenuation of light intensity over distance.
        /// </summary>
        [Description("Factors describing the attenuation of light intensity over distance. (1,0,0) for no attenuation."), Category("Behavior")]
        public Attenuation Attenuation { get; set; }
        #endregion

        //--------------------//

        #region Constructor
        /// <summary>
        /// Creates a new point light with a <see cref="Range"/> of 1000 and no attenuation
        /// </summary>
        public PointLight()
        {
            Range = 1000;
            Attenuation = new Attenuation(1, 0, 0);
        }
        #endregion
    }
}

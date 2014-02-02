/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using AlphaFramework.World.Templates;
using Common.Values;
using LuaInterface;
using SlimDX;

namespace AlphaFramework.World.Components
{
    /// <summary>
    /// Represents a point light source.
    /// </summary>
    /// <seealso cref="EntityTemplateBase{TSelf}.Render"/>
    public class LightSource : Render
    {
        private Color _color = Color.White;

        /// <summary>
        /// The color of this point light source.
        /// </summary>
        /// <remarks>Is not serialized/stored, <see cref="ColorValue"/> is used for that.</remarks>
        [XmlIgnore, LuaHide, Description("The color of this point light source.")]
        public Color Color { get { return _color; } set { _color = Color.FromArgb(255, value); /* Drop alpha-channel */ } }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Color"/>
        [XmlElement("Color"), LuaHide, Browsable(false)]
        public XColor ColorValue { get { return Color; } set { Color = Color.FromArgb(value.R, value.G, value.B); } }

        // Drop alpha-value

        /// <summary>
        /// The maximum distance at which the light source has an effect.
        /// </summary>
        [Description("The maximum distance at which the light source has an effect.")]
        [XmlAttribute]
        public float Range { get; set; }

        private Attenuation _attenuation = new Attenuation(1, 0, 0);

        /// <summary>
        /// Factors describing the attenuation of light intensity over distance.
        /// </summary>
        [DefaultValue(typeof(Vector3), "1; 0; 0"), Description("Factors describing the attenuation of light intensity over distance. (1,0,0) for no attenuation.")]
        public Attenuation Attenuation { get { return _attenuation; } set { _attenuation = value; } }
    }
}

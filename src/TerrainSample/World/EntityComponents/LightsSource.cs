/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using Common.Values;
using LuaInterface;
using SlimDX;
using World.Templates;

namespace World.EntityComponents
{
    /// <summary>
    /// Represents a point light source.
    /// </summary>
    /// <seealso cref="EntityTemplate.RenderControls"/>
    public class LightSource : RenderControl
    {
        private Color _color;

        /// <summary>
        /// The color of this point light source.
        /// </summary>
        /// <remarks>Is not serialized/stored, <see cref="ColorValue"/> is used for that.</remarks>
        [XmlIgnore, LuaHide, Description("The color of this point light source.")]
        public Color Color { get { return _color; } set { _color = Color.FromArgb(255, value); } }

        // Drop alpha-value

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

        #region Constructor
        /// <summary>
        /// Creates a new point light source.
        /// </summary>
        public LightSource()
        {
            Color = Color.White;

            // Hover over the terrain a bit by default
            Shift = new Vector3(0, 20, 0);
        }
        #endregion
    }
}

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
using System.Xml.Serialization;
using AlphaFramework.World.Templates;

namespace AlphaFramework.World.EntityComponents
{
    /// <summary>
    /// Renders a simple (optionally textured) sphere for benchmarks, etc.
    /// </summary>
    /// <seealso cref="EntityTemplateBase{TSelf}.RenderControls"/>
    public class TestSphere : RenderControl
    {
        /// <summary>
        /// The filename of the texture to place on the sphere.
        /// </summary>
        [DefaultValue(""), Description("The filename of the texture to place on the sphere.")]
        [XmlAttribute]
        public string Texture { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            string value = base.ToString();
            if (!string.IsNullOrEmpty(Texture))
                value += ": " + Texture;
            return value;
        }

        private float _radius = 50;

        /// <summary>
        /// The radius of the sphere.
        /// </summary>
        [DefaultValue(50f), Description("The radius of the sphere.")]
        [XmlAttribute]
        public float Radius { get { return _radius; } set { _radius = value; } }

        private int _slices = 50;

        /// <summary>
        /// The number of vertical slices to divide the sphere into.
        /// </summary>
        [DefaultValue(50), Description("The number of vertical slices to divide the sphere into.")]
        [XmlAttribute]
        public int Slices { get { return _slices; } set { _slices = value; } }

        private int _stacks = 50;

        /// <summary>
        /// The number of horizontal stacks to divide the sphere into.
        /// </summary>
        [DefaultValue(50), Description("The number of horizontal stacks to divide the sphere into.")]
        [XmlAttribute]
        public int Stacks { get { return _stacks; } set { _stacks = value; } }

        /// <summary>
        /// The level of transparency from 0 (solid) to 255 (invisible),
        /// 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending.
        /// </summary>
        [DefaultValue(0), Description("The level of transparency from 0 (solid) to 255 (invisible), 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending.")]
        [XmlAttribute]
        public int Alpha { get; set; }
    }
}

/*
 * Copyright 2006-2012 Bastian Eicher
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
using SlimDX;

namespace World.EntityComponents
{
    /// <summary>
    /// Represents a particle system (e.g. fire or steam) controlled by an XML preset.
    /// </summary>
    /// <seealso cref="EntityTemplate.RenderControls"/>
    public abstract class ParticleSystem : RenderControl
    {
        /// <summary>
        /// The filename of the XML particle system preset containing detailed settings.
        /// </summary>
        [Description("The filename of the XML particle system preset containing detailed settings.")]
        [XmlAttribute]
        public string Filename { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            string value = base.ToString();
            if (!string.IsNullOrEmpty(Filename))
                value += ": " + Filename;
            return value;
        }

        /// <summary>
        /// How far this particle system should be visible.
        /// </summary>
        [DefaultValue(0f), Description("How far this particle system should be visible.")]
        [XmlAttribute]
        public float VisibilityDistance { get; set; }

        #region Constructor
        /// <summary>
        /// Creates a new particle system that hovers slightly above the ground.
        /// </summary>
        protected ParticleSystem()
        {
            // Hover over the terrain a bit by default
            Shift = new Vector3(0, 20, 0);
        }
        #endregion
    }
}

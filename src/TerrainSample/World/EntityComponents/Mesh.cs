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
using Common.Values.Design;
using World.Templates;

namespace World.EntityComponents
{

    #region Enumerations
    /// <seealso cref="Mesh.RenderIn"/>
    public enum ViewType
    {
        /// <summary>Render in all types of Views</summary>
        All,

        /// <summary>Do not render in Support Views</summary>
        NormalOnly,

        /// <summary>Render only in Support Views</summary>
        SupportOnly,

        /// <summary>Render only in Support Views for glow maps</summary>
        GlowOnly
    };
    #endregion

    /// <summary>
    /// Represents a mesh loaded from a file.
    /// </summary>
    /// <seealso cref="EntityTemplate.RenderControls"/>
    public abstract class Mesh : RenderControl
    {
        /// <summary>
        /// The filename of the mesh-file to use for rendering.
        /// </summary>
        [DefaultValue(""), Description("The filename of the mesh-file to use for rendering.")]
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
        /// How the mesh loaded from the file shall be rotated around the X axis (east to west).
        /// </summary>
        [DefaultValue(0f), Description("How the mesh loaded from the file shall be rotated around the X axis (east to west).")]
        [EditorAttribute(typeof(AngleEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public float RotationX { get; set; }

        /// <summary>
        /// How the mesh loaded from the file shall be rotated around the Y axis (top to bottom).
        /// </summary>
        [DefaultValue(0f), Description("How the mesh loaded from the file shall be rotated around the Y axis (top to bottom).")]
        [EditorAttribute(typeof(AngleEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public float RotationY { get; set; }

        /// <summary>
        /// How the mesh loaded from the file shall be rotated around the Z axis (north to south).
        /// </summary>
        [DefaultValue(0f), Description("How the mesh loaded from the file shall be rotated around the Z axis (north to south).")]
        [EditorAttribute(typeof(AngleEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public float RotationZ { get; set; }

        private float _scale = 1;

        /// <summary>
        /// A factor by which to scale the mesh loaded from the file.
        /// </summary>
        [DefaultValue(1), Description("A factor by which to scale the mesh loaded from the file.")]
        public float Scale { get { return _scale; } set { _scale = value; } }

        /// <summary>
        /// The level of transparency from 0 (solid) to 255 (invisible),
        /// 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending.
        /// </summary>
        [DefaultValue(0), Description("The level of transparency from 0 (solid) to 255 (invisible), 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending.")]
        [XmlAttribute]
        public int Alpha { get; set; }

        private bool _pickable = true;

        /// <summary>
        /// Can this mesh be picked with the mouse?
        /// </summary>
        [DefaultValue(true), Description("Can this mesh be picked with the mouse?")]
        [XmlAttribute]
        public bool Pickable { get { return _pickable; } set { _pickable = value; } }

        /// <summary>
        /// In what kind of Views shall this mesh be rendered?
        /// </summary>
        [DefaultValue(ViewType.All), Description("In what kind of Views shall this mesh be rendered?"), Category("Behavior")]
        [XmlAttribute]
        public ViewType RenderIn { get; set; }
    }
}

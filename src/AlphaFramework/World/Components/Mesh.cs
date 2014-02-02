/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Drawing.Design;
using System.Xml.Serialization;
using AlphaFramework.World.Templates;
using Common.Values.Design;

namespace AlphaFramework.World.Components
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
    /// <seealso cref="EntityTemplateBase{TSelf}.Render"/>
    public abstract class Mesh : Render
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
        [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
        public float RotationX { get; set; }

        /// <summary>
        /// How the mesh loaded from the file shall be rotated around the Y axis (top to bottom).
        /// </summary>
        [DefaultValue(0f), Description("How the mesh loaded from the file shall be rotated around the Y axis (top to bottom).")]
        [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
        public float RotationY { get; set; }

        /// <summary>
        /// How the mesh loaded from the file shall be rotated around the Z axis (north to south).
        /// </summary>
        [DefaultValue(0f), Description("How the mesh loaded from the file shall be rotated around the Z axis (north to south).")]
        [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
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

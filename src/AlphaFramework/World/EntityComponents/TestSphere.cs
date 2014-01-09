/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
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

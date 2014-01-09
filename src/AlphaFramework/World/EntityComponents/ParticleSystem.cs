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
    /// Represents a particle system (e.g. fire or steam) controlled by an XML preset.
    /// </summary>
    /// <seealso cref="EntityTemplateBase{TSelf}.RenderControls"/>
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
    }
}

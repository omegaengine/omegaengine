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

namespace AlphaFramework.World.Components;

/// <summary>
/// Represents an animated mesh loaded from a file.
/// </summary>
/// <seealso cref="EntityTemplateBase{TSelf}.Render"/>
public class AnimatedMesh : Mesh
{
    /// <summary>
    /// The name of the animation to play by default; empty for the mesh's first animation.
    /// </summary>
    [DefaultValue(""), Description("The name of the animation to play by default; empty for the mesh's first animation.")]
    [XmlAttribute]
    public string? DefaultAnimation { get; set; }
}
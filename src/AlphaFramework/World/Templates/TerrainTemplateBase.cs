/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Xml.Serialization;

namespace AlphaFramework.World.Templates;

/// <summary>
/// A common base for terrain templates. Defines the texture for a patch of terrain.
/// </summary>
public abstract class TerrainTemplateBase<TSelf> : Template<TSelf> where TSelf : TerrainTemplateBase<TSelf>
{
    /// <summary>
    /// The filename of the ground texture.
    /// </summary>
    [XmlAttribute, Browsable(false)]
    public string? Texture { get; set; }
}

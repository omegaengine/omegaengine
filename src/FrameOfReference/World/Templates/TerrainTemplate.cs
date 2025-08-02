/*
 * Copyright 2006-2014 Bastian Eicher
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using AlphaFramework.World.Templates;
using LuaInterface;
using OmegaEngine.Values;

namespace FrameOfReference.World.Templates;

/// <summary>
/// Defines a type of terrain (texture, effects on units, etc.).
/// </summary>
public sealed class TerrainTemplate : Template<TerrainTemplate>
{
    /// <summary>
    /// The filename of the ground texture.
    /// </summary>
    [XmlAttribute, Browsable(false)]
    public string Texture { get; set; }

    private Color _color = Color.Black;

    /// <summary>
    /// The mini-map color for this terrain type. Should be unique.
    /// </summary>
    /// <remarks>Is not serialized/stored, <see cref="ColorValue"/> is used for that.</remarks>
    [XmlIgnore, LuaHide, Description("The mini-map color for this terrain type. Should be unique.")]
    public Color Color { get => _color; set => _color = Color.FromArgb(255, value) /* Drop alpha-channel */; }

    /// <summary>Used for XML serialization.</summary>
    /// <seealso cref="Color"/>
    [XmlElement("Color"), LuaHide, Browsable(false)]
    public XColor ColorValue { get => Color; set => Color = Color.FromArgb(value.R, value.G, value.B) /* Drop alpha-channel */; }

    private float _movementAbility = 1;

    /// <summary>
    /// How well can units walk on this ground? 0=not at all; 1=with full speed
    /// </summary>
    [DefaultValue(1f), Description("How well can units walk on this ground? 0=not at all; 1=with full speed")]
    public float MovementAbility { get => _movementAbility; set => _movementAbility = Math.Max(0, value); }
}
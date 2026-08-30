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
using OmegaEngine.Foundation.Geometry;

namespace AlphaFramework.World.Components;

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
[Cloneable]
public abstract partial class Mesh : Render
{
    /// <summary>
    /// The filename of the mesh-file to use for rendering.
    /// </summary>
    [DefaultValue(""), Description("The filename of the mesh-file to use for rendering.")]
    [XmlAttribute]
    public string? Filename { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string value = base.ToString();
        if (!string.IsNullOrEmpty(Filename))
            value += $": {Filename}";
        return value;
    }

    /// <summary>
    /// How the mesh loaded from the file shall be rotated.
    /// </summary>
    [DefaultValue(typeof(Rotation), "0,0,0"), Description("How the mesh loaded from the file shall be rotated.")]
    public Rotation Rotation { get; set; }

    /// <summary>Used for XML serialization.</summary>
    public bool ShouldSerializeRotation() => Rotation != default;

    /// <summary>
    /// A factor by which to scale the mesh loaded from the file.
    /// </summary>
    [DefaultValue(1f), Description("A factor by which to scale the mesh loaded from the file.")]
    public float Scale { get; set; } = 1;

    /// <summary>
    /// The level of transparency from 0 (solid) to 255 (invisible),
    /// 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending.
    /// </summary>
    [DefaultValue(0), Description("The level of transparency from 0 (solid) to 255 (invisible), 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending.")]
    [XmlAttribute]
    public int Alpha { get; set; }

    /// <summary>
    /// Shall this mesh cast shadows on other objects?
    /// </summary>
    [DefaultValue(false), Description("Shall this mesh cast shadows on other objects?")]
    [XmlAttribute]
    public bool ShadowCaster { get; set; }

    /// <summary>
    /// Shall this mesh receive shadows from other objects?
    /// </summary>
    [DefaultValue(false), Description("Shall this mesh receive shadows from other objects?")]
    [XmlAttribute]
    public bool ShadowReceiver { get; set; }

    /// <summary>
    /// Can this mesh be picked with the mouse?
    /// </summary>
    [DefaultValue(true), Description("Can this mesh be picked with the mouse?")]
    [XmlAttribute]
    public bool Pickable { get; set; } = true;

    /// <summary>
    /// In what kind of Views shall this mesh be rendered?
    /// </summary>
    [DefaultValue(ViewType.All), Description("In what kind of Views shall this mesh be rendered?"), Category("Behavior")]
    [XmlAttribute]
    public ViewType RenderIn { get; set; }
}

using System.ComponentModel;
using System.Xml.Serialization;
using AlphaFramework.World.Templates;

namespace Template.AlphaFramework.World.Templates;

/// <summary>
/// Defines a type of terrain (texture, effects, etc.).
/// </summary>
public sealed class TerrainTemplate : Template<TerrainTemplate>
{
    /// <summary>
    /// The filename of the ground texture.
    /// </summary>
    [XmlAttribute, Browsable(false)]
    public string? Texture { get; set; }
}

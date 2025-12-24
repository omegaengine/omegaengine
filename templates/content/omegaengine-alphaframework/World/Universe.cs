using System.ComponentModel;
using System.Xml.Serialization;
using AlphaFramework.World;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Terrains;
using NanoByte.Common.Collections;
using SlimDX;
using Template.AlphaFramework.World.Templates;

namespace Template.AlphaFramework.World;

/// <summary>
/// Represents a game world with a height-map based <see cref="Terrain"/>.
/// </summary>
public sealed class Universe : CoordinateUniverse<Vector2>
{
    /// <inheritdoc/>
    [Browsable(false)]
    [XmlElement(typeof(CameraState<Vector2>), ElementName = "CameraState")]
    public override MonitoredCollection<Positionable<Vector2>> Positionables { get; } = [];

    /// <summary>
    /// The <see cref="Terrain"/> on which entities are placed.
    /// </summary>
    [XmlIgnore, Browsable(false)]
    public Terrain<TerrainTemplate>? Terrain { get; set; }

    /// <summary>
    /// Creates a new <see cref="Universe"/> with a terrain.
    /// </summary>
    /// <param name="terrain">The terrain for the new <see cref="Universe"/>.</param>
    public Universe(Terrain<TerrainTemplate> terrain)
    {
        Terrain = terrain;
    }

    /// <summary>
    /// Base-constructor for XML serialization. Do not call manually!
    /// </summary>
    public Universe()
    {}
}

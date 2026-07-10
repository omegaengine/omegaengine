using System;
using System.IO;
using System.Xml.Serialization;
using AlphaFramework.World;
using AlphaFramework.World.Positionables;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using OmegaEngine.Foundation.Storage;
using SlimDX;

namespace Template.AlphaFramework.World;

/// <summary>
/// Represents a game world. Corresponds to the content of a map file.
/// </summary>
public sealed class Universe : CoordinateUniverse<Vector3>
{
    /// <inheritdoc/>
    // List every serializable Positionable subtype here so the XML serializer knows about it.
    [XmlElement(typeof(Entity))]
    public override MonitoredCollection<Positionable<Vector3>> Positionables { get; } = new();

    /// <summary>
    /// Loads a <see cref="Universe"/> from an XML map file.
    /// </summary>
    /// <param name="path">The file to load from.</param>
    /// <exception cref="IOException">A problem occurred while reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    /// <exception cref="InvalidOperationException">A problem occurred while deserializing the XML data.</exception>
    public static Universe Load(string path)
    {
        var universe = XmlStorage.LoadXml<Universe>(path);
        universe.PostLoad(path);
        return universe;
    }

    /// <summary>
    /// Loads a <see cref="Universe"/> from the game content source via the <see cref="ContentManager"/>.
    /// </summary>
    /// <param name="id">The ID of the map to load (e.g. <c>Demo.map</c>).</param>
    public static Universe FromContent(string id)
    {
        using var stream = ContentManager.GetFileStream("World/Maps", id);
        var universe = XmlStorage.LoadXml<Universe>(stream);
        universe.PostLoad(id);
        return universe;
    }

    public override void Update(double elapsedGameTime)
    {
        base.Update(elapsedGameTime);

        // Add your own gameplay logic here.
    }
}

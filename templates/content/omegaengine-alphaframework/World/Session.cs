using System;
using System.IO;
using AlphaFramework.World;
using JetBrains.Annotations;

namespace Template.AlphaFramework.World;

/// <summary>
/// State of a game session. Corresponds to a savegame.
/// </summary>
public sealed class Session : Session<Universe>
{
    /// <summary>
    /// Creates a new game session.
    /// </summary>
    /// <param name="universe">Contents of the game world.</param>
    public Session(Universe universe) : base(universe)
    {}

    /// <summary>
    /// Used for XML serialization. Do not call manually!
    /// </summary>
    [UsedImplicitly, Obsolete("Used for XML serialization. Do not call manually!")]
    public Session()
    {}

    /// <summary>
    /// Loads a <see cref="Session"/> from a compressed XML file (savegame).
    /// </summary>
    /// <param name="path">The file to load from.</param>
    /// <exception cref="IOException">A problem occurred while reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    /// <exception cref="InvalidOperationException">A problem occurred while deserializing the XML data.</exception>
    public new static Session Load(string path)
        => Load<Session>(path);
}

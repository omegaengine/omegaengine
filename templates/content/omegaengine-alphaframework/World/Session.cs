using AlphaFramework.World;

namespace Template.AlphaFramework.World;

/// <summary>
/// Represents a game session (i.e. a game actually being played).
/// It is equivalent to the content of a savegame.
/// </summary>
public sealed class Session : Session<Universe>
{
    /// <summary>
    /// Creates a new game session based upon a given <see cref="Universe"/>.
    /// </summary>
    /// <param name="baseUniverse">The universe to base the new game session on.</param>
    public Session(Universe baseUniverse) : base(baseUniverse)
    {}

    /// <summary>
    /// Base-constructor for XML serialization. Do not call manually!
    /// </summary>
    public Session()
    {}
}

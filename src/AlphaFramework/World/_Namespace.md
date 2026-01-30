---
uid: AlphaFramework.World
summary: Provides a basis for building engine-agnostic models of game worlds.
---
> [!NOTE]
> NuGet package: [AlphaFramework.World](https://www.nuget.org/packages/AlphaFramework.World/)

## Universe

The **Universe** (see <xref:AlphaFramework.World.UniverseBase>) represents the game world data. It contains:

- **Entities and objects** - All game objects, their properties, and relationships
- **Terrain data** - Heightmaps, textures, and world geometry

The Universe is typically serialized to/from files as level or map data. It is designed to be presentation-agnostic and can be edited without running the game.

```csharp
public class Universe : UniverseBase
{
    public static Universe FromContent(string id)
        => Load(ContentManager.GetFilePath("World/Maps", id));

    // Serializable properties describing game world here

    public override void Update(double elapsedGameTime)
    {
        base.Update(elapsedGameTime);

        // Game world update logic here
    }
}
```

## Session

A **Session** (see <xref:AlphaFramework.World.Session`1>) represents a game being played. It wraps a Universe and adds:

- **Dynamic state** - Entity states that change during gameplay
- **Runtime data** - Player progress, inventory, quest states
- **Temporal information** - Game time, event history

Sessions are serializable as savegames.

```csharp
public class Session : Session<Universe>
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

    // Serializable properties describing game state here
}
```

## API

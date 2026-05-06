---
uid: AlphaFramework.World
summary: Provides a basis for building engine-agnostic models of game worlds.
---
> [!NOTE]
> NuGet package: [AlphaFramework.World](https://www.nuget.org/packages/AlphaFramework.World/)

## Universe

A **Universe** represents the game world data. It can contain:

- **Entities**: Game objects, their properties, and relationships
- **Terrain data**: Heightmaps, textures, world geometry
- Any other data that is relevant to the game

A Universe is typically serialized to/from files as level or map data. It is designed to be presentation-agnostic and can be edited without running the game.

Derive from <xref:AlphaFramework.World.UniverseBase> to define your own Universe type:

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

A **Session** represents a game being played. Sessions are serializable as savegames.

You can use the generic <xref:AlphaFramework.World.Session`1> class to create sessions that simply wrap a Universe.

Or you can derive from the type to extend it with your own properties for things like player progress, inventory, or quest states:

```csharp
public class Session : Session<Universe>
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

    // Serializable properties describing game state here
}
```

## API

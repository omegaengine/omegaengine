---
uid: AlphaFramework.World.Templates
summary: Template system for building reusable/instantiable entities out of <xref:AlphaFramework.World.Components>.
---
AlphaFramework's template system allows you to build reusable, instantiable entities from <xref:AlphaFramework.World.Components>. Templates aggregate multiple components into a single definition that can be referenced by name and instantiated multiple times throughout your game world.

TODO: review

## Template types

AlphaFramework provides two main template base classes:

- **<xref:AlphaFramework.World.Templates.EntityTemplateBase`1>** - Templates for game entities (characters, objects, etc.). Contains rendering components, collision data, and behavior properties.
- **<xref:AlphaFramework.World.Templates.TerrainTemplateBase`1>** - Templates for terrain types. Defines textures and properties for terrain tiles. See <xref:AlphaFramework.World.Terrains> for details on terrain system usage.

Derive from `EntityTemplateBase<>` to create your own entity template types:

```csharp
public class EntityTemplate : EntityTemplateBase<EntityTemplate>
{
    // Add custom properties specific to your game
}
```

## Template structure

Templates inherit from <xref:AlphaFramework.World.Templates.Template`1> and define:

- **Name** - Unique identifier for referencing the template
- **Components** - Collection of components that make up the entity
- **Properties** - Additional metadata like highlight color

## Usage

Derive from the common base class to define your own template type

```csharp
public class EntityTemplate : EntityTemplateBase<EntityTemplate>
{
    // Add custom properties here
}
```

Instantiate a template:

```csharp
var characterTemplate = new EntityTemplate
{
    Name = "Guard",
    Render =
    {
        new StaticMesh { Filename = "Guard.x" },
        new LightSource { Color = Color.White }
    },
    Collision = new Circle { Radius = 0.5f }
};
```

Reference it multiple times in your world:

```csharp
universe.Positionables.Add(new Entity<EntityTemplate>
{
    TemplateData = characterTemplate,
    Position = new Vector2(10, 5)
});
```

## Storage system

Templates are loaded from XML files using the storage system:

**<xref:AlphaFramework.World.Templates.Template`1.LoadAll>** - Loads all templates of a given type from the template file (e.g., `World/EntityTemplate.xml`):

```csharp
// Load all entity templates from storage
EntityTemplate.LoadAll();

// Access loaded templates
var guardTemplate = EntityTemplate.All["Guard"];
```

**Entity.Template** - Entities reference templates by name. When an entity has a `TemplateName` property set, it can retrieve its template data:

```csharp
var entity = new Entity<EntityTemplate>
{
    TemplateName = "Guard",
    Position = new Vector2(10, 5)
};

// Access template data
var template = entity.Template; // Retrieves "Guard" template from EntityTemplate.All
```

This system allows you to define templates once in XML files and reference them multiple times by name throughout your game world, promoting reusability and easier content management.

## API

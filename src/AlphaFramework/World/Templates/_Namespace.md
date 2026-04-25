---
uid: AlphaFramework.World.Templates
summary: Template system for building reusable/instantiable entities out of <xref:AlphaFramework.World.Components>.
---
A template is created once (typically loaded from XML) and can be referenced by name from many entity instances throughout your game world.

## Template types

AlphaFramework provides two main template base classes:

- **<xref:AlphaFramework.World.Templates.EntityTemplateBase`1>** - Templates for game entities (characters, objects, vehicles, etc.). Contains a collection of <xref:AlphaFramework.World.Templates.EntityTemplateBase`1.Render> components and an optional <xref:AlphaFramework.World.Components.Movement> component.
- **<xref:AlphaFramework.World.Templates.TerrainTemplateBase`1>** - Templates for terrain types. Defines the texture and traversal properties for terrain tiles. See <xref:AlphaFramework.World.Terrains> for details on the terrain system.

## Extending templates

Derive from <xref:AlphaFramework.World.Templates.EntityTemplateBase`1> using the self-referential generic pattern to create your own template type. Add any game-specific properties (e.g. collision shapes, hitpoints) alongside the inherited rendering and movement data:

```csharp
public class EntityTemplate : EntityTemplateBase<EntityTemplate>
{
    [XmlElement(typeof(Circle)), XmlElement(typeof(Box))]
    public Collision<Vector2>? Collision { get; set; }
}
```

For more specialized hierarchies you can also derive directly from <xref:AlphaFramework.World.Templates.Template`1> and compose your own component collections instead of using the built-in `Render` list.

## Render components

`EntityTemplateBase<T>.Render` is a polymorphic list of <xref:AlphaFramework.World.Components.Render> instances that describe how the entity looks:

| Component                                                | Description                                                 |
| -------------------------------------------------------- | ----------------------------------------------------------- |
| <xref:AlphaFramework.World.Components.StaticMesh>        | A rigid mesh loaded from an `.x` file                       |
| <xref:AlphaFramework.World.Components.AnimatedMesh>      | A skinned/animated mesh loaded from an `.x` file            |
| <xref:AlphaFramework.World.Components.TestSphere>        | A procedurally generated sphere, useful for prototyping     |
| <xref:AlphaFramework.World.Components.CpuParticleSystem> | A CPU-driven particle effect loaded from an XML preset file |
| <xref:AlphaFramework.World.Components.LightSource>       | A point light with colour and distance attenuation          |

All render components support a `Shift` offset relative to the entity origin. <xref:AlphaFramework.World.Components.Mesh>-based components additionally support `Scale`, per-axis rotation, `Alpha` and `ShadowCaster`/`ShadowReceiver` flags.

## Creating a template in code

```csharp
var guardTemplate = new EntityTemplate
{
    Name = "Guard",
    Render =
    {
        new StaticMesh { Filename = "Guard.x", Scale = 0.5f },
        new LightSource { Color = Color.White, Attenuation = new Attenuation(1, 0, 0) }
    },
    Collision = new Circle { Radius = 0.5f }
};
```

## Storage

Templates are stored in and loaded from XML files. By convention each template type is stored in a file named after the class with an `s` suffix, e.g. `EntityTemplates.xml`.

<xref:AlphaFramework.World.Templates.Template`1.LoadAll> populates the static <xref:AlphaFramework.World.Templates.Template`1.All> collection:

```csharp
EntityTemplate.LoadAll(); // reads EntityTemplates.xml
var guard = EntityTemplate.All["Guard"];
```

## Binding templates to entities

Entities reference their template by name via `TemplateName`. Assigning this property looks up the template in <xref:AlphaFramework.World.Templates.Template`1.All> and **deep-clones** it into `TemplateData`, so each entity owns an independent copy that can be modified at runtime without affecting other instances:

```csharp
var entity = new Entity<EntityTemplate>
{
    TemplateName = "Guard", // clones the template into TemplateData
    Position = new Vector2(10, 5)
};

// Access the cloned template data at runtime
float radius = entity.TemplateData?.Collision is Circle c ? c.Radius : 0f;
```

In savegame files `TemplateData` is serialized directly (overriding the map-file template reference on load), which allows per-entity customization to persist across sessions.

## Rendering bridge

<xref:AlphaFramework.Presentation> provides extension methods that convert the data-side render components into live OmegaEngine renderables.

## API

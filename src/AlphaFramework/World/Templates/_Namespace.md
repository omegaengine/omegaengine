---
uid: AlphaFramework.World.Templates
summary: Template system for building reusable/instantiable entities out of <xref:AlphaFramework.World.Components>.
---
TODO: review

AlphaFramework's template system allows you to build reusable, instantiable entities from <xref:AlphaFramework.World.Components>. Templates aggregate multiple components into a single definition that can be referenced by name and instantiated multiple times throughout your game world.

## Template structure

Templates inherit from <xref:AlphaFramework.World.Templates.Template`1> and define:

- **Name** - Unique identifier for referencing the template
- **Components** - Collection of components that make up the entity
- **Properties** - Additional metadata like highlight color

## Usage

Define a template once:

```csharp
var characterTemplate = new EntityTemplate
{
    Name = "Guard",
    Components =
    {
        new Mesh { Filename = "Guard.x" },
        new Collision { Height = 1.8f, Radius = 0.5f },
        new LightSource { Range = 5, Diffuse = Color.White }
    }
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

## API

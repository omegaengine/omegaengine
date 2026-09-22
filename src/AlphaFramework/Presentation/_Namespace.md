---
uid: AlphaFramework.Presentation
summary: Provides a basis for building presenters that visualize <xref:AlphaFramework.World>-based game worlds using <xref:OmegaEngine.Graphics.Renderables>.
---
> [!NOTE]
> NuGet package: [AlphaFramework.Presentation](https://www.nuget.org/packages/AlphaFramework.Presentation/)

## Presenter

The **Presenter** (see <xref:AlphaFramework.Presentation.PresenterBase`1>) handles visual representation. It bridges the game world and the rendering engine:

- **Creates renderables** - Converts entities to [Renderables](xref:OmegaEngine.Graphics.Renderables)
- **Manages the scene** - Adds/removes objects from the <xref:OmegaEngine.Graphics.Scene>
- **Handles view updates** - Keeps visual representation synchronized with world state

Different presenter implementations can provide different visualization modes (e.g., in-game, editor, menu background) without changing the underlying world data.

```csharp
public class Presenter : PresenterBase<Universe>
{
    public Presenter(Engine engine, Universe universe) : base(engine, universe)
    {
        View = new(Scene, new FreeFlyCamera());
    }

    public override void Initialize()
    {
        if (Initialized) return;

        // Add PositionableRenderables to Scene.Positionables

        base.Initialize();
    }
}
```

## Template rendering

<xref:AlphaFramework.Presentation.RenderComponentPresentation> provides `.ToPresentation()` extension methods that convert AlphaFramework <xref:AlphaFramework.World.Templates.EntityTemplateBase`1.Render> components into live OmegaEngine renderables:

| Data component                                           | Presentation type                                          |
| -------------------------------------------------------- | ---------------------------------------------------------- |
| <xref:AlphaFramework.World.Components.StaticMesh>        | <xref:OmegaEngine.Graphics.Renderables.Model>              |
| <xref:AlphaFramework.World.Components.AnimatedMesh>      | <xref:OmegaEngine.Graphics.Renderables.AnimatedModel>      |
| <xref:AlphaFramework.World.Components.TestSphere>        | <xref:OmegaEngine.Graphics.Renderables.Model> (procedural) |
| <xref:AlphaFramework.World.Components.CpuParticleSystem> | <xref:OmegaEngine.Graphics.Renderables.CpuParticleSystem>  |
| <xref:AlphaFramework.World.Components.LightSource>       | <xref:OmegaEngine.Graphics.LightSources.PointLight>        |

This is useful for applying <xref:AlphaFramework.World.Templates>. A presenter typically iterates `entity.TemplateData.Render`, calls `.ToPresentation(engine)` on each component, and adds the resulting renderables to the entity's group in <xref:AlphaFramework.Presentation.PivotedRenderables>:

```csharp
foreach (var component in entity.TemplateData!.Render)
{
    switch (component)
    {
        case StaticMesh m when m.ToPresentation(engine) is {} model:
            renderables.AddTo(model, entity, entity.Name);
            break;
        case LightSource l:
            var light = l.ToPresentation();
            light.AttachedTo = renderables.PivotFor(entity, entity.Name);
            scene.Lights.Add(light);
            break;
    }
}
```

A component's `Shift` becomes the renderable's local <xref:OmegaEngine.Graphics.Renderables.PositionableRenderable.Position> (or the light's `Offset`), so writing the entity's position and rotation to `renderables.AnchorFor(entity)` moves everything belonging to the entity at once. The group only gets a <xref:OmegaEngine.Graphics.Renderables.Pivot> of its own once more than one thing shares that transform. See <xref:AlphaFramework> for the full hierarchy contract.

## <xref:AlphaFramework.Presentation.GameBase>

**GameBase** provides the application shell. It extends <xref:OmegaEngine.RenderHost> with game-specific features:

- **Settings management** - Loads and saves game configuration
- **GUI system** - Integrates OmegaGUI for menus and HUD
- **Lifecycle management** - Handles initialization, main loop, and cleanup

Your game class derives from `GameBase` and coordinates the other components:

```csharp
public class MyGame(Settings settings)
    : GameBase(settings, "My game")
{
    private Session? _session;
    private Presenter? _presenter;

    protected override bool Initialize()
    {
        if (!base.Initialize()) return false;

        // Load universe
        var universe = Universe.Load("Level1.xml");

        // Create session
        _session = new Session(universe);

        // Create presenter
        _presenter = new(Engine, universe);
        _presenter.HookIn();

        return true;
    }

    protected override void Render(double elapsedTime)
    {
        _session!.Update(elapsedTime);
        base.Render(elapsedTime);
    }
}
```

## API

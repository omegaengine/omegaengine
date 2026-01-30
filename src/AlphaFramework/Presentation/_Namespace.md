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
public class Presenter(Engine engine, Universe universe) : PresenterBase<Universe>
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

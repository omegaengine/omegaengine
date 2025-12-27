---
uid: AlphaFramework.Presentation
summary: *content
---
Provides a basis for building presenters that visualize <xref:AlphaFramework.World>-based game worlds using <xref:OmegaEngine.Graphics.Renderables>.

**NuGet package:** [AlphaFramework.Presentation](https://www.nuget.org/packages/AlphaFramework.Presentation/)

### Presenter

The **Presenter** (see <xref:AlphaFramework.Presentation.PresenterBase`1>) handles visual representation. It bridges the game world and the rendering engine:

- **Creates renderables** - Converts entities to <xref:OmegaEngine.Graphics.Renderables>s
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

### <xref:AlphaFramework.Presentation.GameBase>

**GameBase** provides the application shell. It extends <xref:OmegaEngine.RenderHost> with game-specific features:

- **Settings management** - Loads and saves game configuration
- **GUI system** - Integrates OmegaGUI for menus and HUD
- **Lifecycle management** - Handles initialization, main loop, and cleanup

Your game class derives from `GameBase` and coordinates the other components:

TODO: Simplify code sample

```csharp
public class MyGame : GameBase
{
    private Session? _currentSession;
    private Presenter? _currentPresenter;

    protected override bool Initialize()
    {
        if (!base.Initialize()) return false;

        // Load universe
        var universe = Universe.Load("Maps/Level1.xml");

        // Create session
        _currentSession = new Session(universe);

        // Create presenter
        _currentPresenter = new Presenter(Engine, universe);
        _currentPresenter.Initialize();

        return true;
    }

    protected override void Render()
    {
        _currentPresenter?.Render();
        base.Render();
    }
}
```

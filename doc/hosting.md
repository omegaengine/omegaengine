# Hosting

For simple use cases, you initialize the Engine and call the `.Render()` method yourself. However, for most use-cases you will probably want to use one of the built-in hosting classes.

## RenderPanel

<xref:OmegaEngine.RenderPanel> is a Windows Forms UserControl that hosts the engine.

- **Embeddable** - Can be placed in forms with other controls
- **Optional render loop** - Choose between automatic rendering or manual control
- **Input system** - Hooks up [input providers](xref:OmegaEngine.Input) (mouse, keyboard, etc.)

**Use when:** Integrating 3D rendering into a level editor, model viewer, etc..

```csharp
// In your Form designer
var renderPanel = new RenderPanel
{
    AutoRender = true,
    AutoRenderInterval = 33 // ~30 FPS
};
this.Controls.Add(renderPanel);

// In your Form_Load method
renderPanel.Setup();
var scene = new Scene { /* ... */ };
var camera = new ArcballCamera();
renderPanel.Engine.Views.Add(new View(scene, camera));
renderPanel.AddInputReceiver(camera);
```

## RenderHost

<xref:OmegaEngine.RenderHost> is a application host that provides a windowed or fullscreen application.

- **Window management** - Creates and manages a render-capable form with fullscreen support
- **Render loop** - Handles the main application loop and frame rendering
- **Input system** - Hooks up [input providers](xref:OmegaEngine.Input) (mouse, keyboard, etc.)

**Use when:** Building a standalone game or application that needs its own window.

```csharp
public class MyApp : RenderHost(name: "My Application")
{
    protected override bool Initialize()
    {
        if (!base.Initialize()) return false;

        var scene = new Scene { /* ... */ };
        var camera = new ArcballCamera();
        Engine.Views.Add(new View(scene, camera));

        this.AddInputReceiver(camera);

        return true;
    }
}

static class Program
{
    static void Main()
    {
        using var app = new MyApp();
        app.Run();
    }
}
```

## GameBase

<xref:AlphaFramework.Presentation.GameBase> extends `RenderHost` with <xref:AlphaFramework>-specific features:

- **GUI system** - Automatically initializes <xref:OmegaGUI>
- **Settings management** - Automatically applies settings to the engine and input system when they change

**Use when:** Building a game using AlphaFramework.

```csharp
public class MyGame(Settings settings) : GameBase(settings, name: "My Application")
{
    protected override bool Initialize()
    {
        if (!base.Initialize()) return false;

        var scene = new Scene { /* ... */ };
        var camera = new ArcballCamera();
        Engine.Views.Add(new View(scene, camera));

        this.AddInputReceiver(camera);

        LoadDialog("MainMenu");

        return true;
    }
}

static class Program
{
    static void Main()
    {
        using var game = new MyGame(Settings.Current);
        game.Run();
    }
}
```

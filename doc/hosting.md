# Hosting

TODO: review

You can wire up Engine and render loop yourself, but...

OmegaEngine provides several hosting classes that handle window management, engine initialization, and render loop coordination. Choose the appropriate hosting class based on your application type.

- **Engine initialization** - Automatically creates and configures the <xref:OmegaEngine.Engine>
- Drive render loop
- **Input handling** - Sets up the [input system](xref:OmegaEngine.Input)

## RenderPanel

<xref:OmegaEngine.RenderPanel> is a Windows Forms UserControl that hosts the engine within a larger application:

- **Embeddable** - Can be placed in forms with other controls
- **Optional render loop** - Choose between automatic rendering or manual control

**Use when:** Integrating 3D rendering into a level editor, model viewer, etc..

```csharp
// In your Form designer
var renderPanel = new RenderPanel
{
    AutoRender = true,
    AutoRenderInterval = 33 // ~30 FPS
};
this.Controls.Add(renderPanel);

// Initialize the engine
renderPanel.Setup();

var view = new View(new Scene(), new ArcballCamera());
renderPanel.Engine.Views.Add(view);

// Load and display a model
var model = new Model(XMesh.Get(engine, "MyModel.x"));
view.Scene.Renderables.Add(model);
```

## RenderHost

<xref:OmegaEngine.RenderHost> is a application host that provides a windowed application with:

- **Window management** - Creates and manages a render-capable form with fullscreen support
- **Render loop** - Handles the main application loop and frame rendering
- **Debug console** - Provides access to debugging features

**Use when:** Building a standalone game or application that needs its own window.

```csharp
public class MyApp : RenderHost
{
    public MyApp() : base("My Application", icon: Resources.AppIcon)
    { }

    protected override bool Initialize()
    {
        if (!base.Initialize()) return false;

        var view = new View(new Scene(), new FreeFlyCamera());
        Engine.Views.Add(view);

        return true;
    }

    protected override void Render()
    {
        // Custom rendering logic here
        base.Render();
    }
}

static void Main()
{
    using var app = new MyApp();
    app.ToWindowed();
    app.Run();
}
```

## GameBase

<xref:AlphaFramework.Presentation.GameBase> extends `RenderHost` with AlphaFramework-specific features:

- **Settings management** - Loads and saves game configuration
- **GUI system** - Integrates OmegaGUI via <xref:OmegaGUI.GuiManager>

**Use when:** Building a game using <xref:AlphaFramework>.
